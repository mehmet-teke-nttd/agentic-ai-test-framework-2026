using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Execution.Commands;

public interface IExecutionCommandHandler
{
    ExecutionKeyword Keyword { get; }

    Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
