using System.Diagnostics;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class StepExecutor : IStepExecutor
{
    private readonly ILogger<StepExecutor> _logger;
    private readonly IExecutionCommandHandlerResolver _handlerResolver;

    public StepExecutor(
        ILogger<StepExecutor> logger,
        IExecutionCommandHandlerResolver handlerResolver)
    {
        _logger = logger;
        _handlerResolver = handlerResolver;
    }

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(context);

        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var handler = _handlerResolver.Resolve(step.Keyword);
            var result = await handler.ExecuteAsync(step, context, cancellationToken);
            stopwatch.Stop();

            result.TestId = context.TestIntent.TestId;
            result.Step = step.Step;
            result.Keyword = step.Keyword;
            result.Target ??= step.Target;
            result.StartedAt = startedAt;
            result.DurationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Step completed with {Status} for Step={Step}, Keyword={Keyword}",
                result.Status,
                step.Step,
                step.Keyword);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unhandled exception while executing Step={Step}, Keyword={Keyword}", step.Step, step.Keyword);

            var errorCode = ex is InvalidOperationException
                ? RuntimeExecutionErrorCode.UnsupportedHandler
                : RuntimeExecutionErrorCode.PlaywrightActionFailed;

            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Blocked,
                StartedAt = startedAt,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ExecutionErrorFactory.FromRuntime(
                    errorCode,
                    errorCode == RuntimeExecutionErrorCode.UnsupportedHandler
                        ? "No handler is registered for the execution keyword."
                        : "Step execution failed due to a runtime automation error.")
            };
        }
    }
}
