using System.Diagnostics;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class ExecutionEngine : IExecutionEngine
{
    private readonly ILogger<ExecutionEngine> _logger;
    private readonly ITestIntentValidator _testIntentValidator;
    private readonly IExecutionContractValidator _executionContractValidator;
    private readonly IStepExecutor _stepExecutor;
    private readonly ITestResultAggregator _testResultAggregator;
    private readonly ITestResultWriter _testResultWriter;

    public ExecutionEngine(
        ILogger<ExecutionEngine> logger,
        ITestIntentValidator testIntentValidator,
        IExecutionContractValidator executionContractValidator,
        IStepExecutor stepExecutor,
        ITestResultAggregator testResultAggregator,
        ITestResultWriter testResultWriter)
    {
        _logger = logger;
        _testIntentValidator = testIntentValidator;
        _executionContractValidator = executionContractValidator;
        _stepExecutor = stepExecutor;
        _testResultAggregator = testResultAggregator;
        _testResultWriter = testResultWriter;
    }

    public async Task<TestExecutionResult> ExecuteAsync(
        ExecutionContract executionContract,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(executionContract);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TestIntent);

        var startedAt = DateTimeOffset.UtcNow;
        var timer = Stopwatch.StartNew();
        _logger.LogInformation("Test execution started for TestId={TestId}", executionContract.TestId);

        ExecutionError? preExecutionError = ValidatePreExecution(executionContract, context);
        var orderedSteps = executionContract.Steps?
            .OrderBy(step => step.Step)
            .ToList() ?? [];

        if (preExecutionError is not null)
        {
            timer.Stop();
            var blockedSkipped = BuildSkippedSteps(
                orderedSteps,
                SkipReason.DependencyNotMet,
                null,
                context.TestIntent.TestId);
            var blockedResult = _testResultAggregator.Aggregate(
                context.TestIntent.TestId,
                blockedSkipped,
                startedAt,
                DateTimeOffset.UtcNow,
                timer.ElapsedMilliseconds,
                preExecutionError);
            await _testResultWriter.WriteAsync(blockedResult, CancellationToken.None);
            return blockedResult;
        }

        var results = new List<StepExecutionResult>();
        StepExecutionResult? terminationStep = null;

        foreach (var step in orderedSteps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Execution cancelled before Step={Step}", step.Step);
                break;
            }

            _logger.LogInformation("Executing Step={Step}", step.Step);
            var result = await _stepExecutor.ExecuteAsync(step, context, cancellationToken);
            results.Add(result);

            if (result.Status is StepStatus.Failed or StepStatus.Blocked or StepStatus.Skipped)
            {
                terminationStep = result;
                _logger.LogInformation("Stopping dependent execution after Step={Step} with Status={Status}", result.Step, result.Status);
                break;
            }
        }

        if (terminationStep is not null)
        {
            var completedSteps = results.Count;
            var remainingSteps = orderedSteps.Skip(completedSteps).ToList();
            var skipReason = terminationStep.Status switch
            {
                StepStatus.Failed => SkipReason.PreviousStepFailed,
                StepStatus.Blocked => SkipReason.PreviousStepBlocked,
                _ => SkipReason.DependencyNotMet
            };

            var skipped = BuildSkippedSteps(
                remainingSteps,
                skipReason,
                terminationStep.Step,
                context.TestIntent.TestId);

            if (skipped.Count > 0)
            {
                _logger.LogInformation("Marking remaining steps skipped. Count={Count}", skipped.Count);
            }

            results.AddRange(skipped);
        }
        else if (results.Count < orderedSteps.Count)
        {
            var remaining = orderedSteps.Skip(results.Count).ToList();
            var skipped = BuildSkippedSteps(
                remaining,
                SkipReason.DependencyNotMet,
                results.LastOrDefault()?.Step,
                context.TestIntent.TestId);
            results.AddRange(skipped);
            preExecutionError ??= ExecutionErrorFactory.FromTest(
                TestExecutionErrorCode.ExecutionCancelled,
                "Execution cancelled.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            preExecutionError ??= ExecutionErrorFactory.FromTest(
                TestExecutionErrorCode.ExecutionCancelled,
                "Execution cancelled.");
        }

        timer.Stop();
        var finalResult = _testResultAggregator.Aggregate(
            context.TestIntent.TestId,
            results,
            startedAt,
            DateTimeOffset.UtcNow,
            timer.ElapsedMilliseconds,
            preExecutionError);

        await _testResultWriter.WriteAsync(finalResult, CancellationToken.None);
        _logger.LogInformation("Test execution completed: {Status}", finalResult.Status);
        return finalResult;
    }

    private ExecutionError? ValidatePreExecution(ExecutionContract executionContract, ExecutionContext context)
    {
        try
        {
            var intentValidation = _testIntentValidator.Validate(context.TestIntent);
            if (intentValidation.Status != ValidationStatus.Valid)
            {
                return ExecutionErrorFactory.FromTest(
                    TestExecutionErrorCode.TestIntentInvalid,
                    "Test intent validation failed.");
            }

            var contractValidation = _executionContractValidator.Validate(executionContract, context.TestIntent);
            if (!contractValidation.IsValid)
            {
                return ExecutionErrorFactory.FromTest(
                    TestExecutionErrorCode.ExecutionContractInvalid,
                    "Execution contract validation failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-execution validation failed due to configuration/runtime issue.");
            return ExecutionErrorFactory.FromTest(
                TestExecutionErrorCode.ExecutionConfigurationError,
                "Execution configuration validation failed.");
        }

        return null;
    }

    private static List<StepExecutionResult> BuildSkippedSteps(
        IReadOnlyList<ExecutionStep> remainingSteps,
        SkipReason reason,
        int? dependsOn,
        string testId)
    {
        return remainingSteps
            .Select(step => new StepExecutionResult
            {
                TestId = testId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Skipped,
                StartedAt = null,
                DurationMs = 0,
                SkipReason = reason,
                DependsOn = dependsOn,
                Error = null
            })
            .ToList();
    }
}
