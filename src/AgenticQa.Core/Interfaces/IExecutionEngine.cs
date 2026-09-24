using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IExecutionEngine
{
    Task<TestExecutionResult> ExecuteAsync(
        ExecutionContract executionContract,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
