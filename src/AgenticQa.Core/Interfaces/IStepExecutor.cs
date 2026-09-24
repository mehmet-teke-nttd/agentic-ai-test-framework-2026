using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IStepExecutor
{
    Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
