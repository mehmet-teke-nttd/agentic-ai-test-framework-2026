using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Commands;

public sealed class VerifyUrlCommandHandler : IExecutionCommandHandler
{
    private readonly ILogger<VerifyUrlCommandHandler> _logger;
    private readonly IPlaywrightAssertionService _assertionService;
    private readonly ITestDataResolver _testDataResolver;

    public VerifyUrlCommandHandler(
        ILogger<VerifyUrlCommandHandler> logger,
        IPlaywrightAssertionService assertionService,
        ITestDataResolver testDataResolver)
    {
        _logger = logger;
        _assertionService = assertionService;
        _testDataResolver = testDataResolver;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.VerifyUrl;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: VERIFY_URL", step.Step);

        if (string.IsNullOrWhiteSpace(step.Expected))
        {
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Status = StepStatus.Blocked,
                Error = ExecutionErrorFactory.FromRuntime(
                    RuntimeExecutionErrorCode.PlaywrightActionFailed,
                    "Verify URL step requires an expected value.")
            };
        }

        var resolvedExpected = _testDataResolver.Resolve(step.Expected, context.TestIntent);
        if (!resolvedExpected.IsSuccess || resolvedExpected.ResolvedValue is null)
        {
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Status = StepStatus.Blocked,
                Error = ExecutionErrorFactory.FromRuntime(
                    resolvedExpected.ErrorCode ?? RuntimeExecutionErrorCode.TestDataReferenceNotFound,
                    resolvedExpected.ErrorMessage ?? "Expected URL reference could not be resolved.")
            };
        }

        try
        {
            await _assertionService.AssertUrlAsync(context.Page, resolvedExpected.ResolvedValue);
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Status = StepStatus.Passed,
                Error = null
            };
        }
        catch (AssertionFailedException ex)
        {
            _logger.LogWarning(ex, "URL assertion failed");
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Status = StepStatus.Failed,
                Error = ExecutionErrorFactory.FromRuntime(
                    RuntimeExecutionErrorCode.AssertionFailed,
                    "Expected URL did not match.")
            };
        }
    }
}
