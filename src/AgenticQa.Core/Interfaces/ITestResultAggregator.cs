using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestResultAggregator
{
    TestExecutionResult Aggregate(
        string testId,
        IReadOnlyList<StepExecutionResult> stepResults,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        long durationMs,
        ExecutionError? testLevelError = null);
}
