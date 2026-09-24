using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class TestResultAggregator : ITestResultAggregator
{
    private readonly ILogger<TestResultAggregator> _logger;

    public TestResultAggregator(ILogger<TestResultAggregator> logger)
    {
        _logger = logger;
    }

    public TestExecutionResult Aggregate(
        string testId,
        IReadOnlyList<StepExecutionResult> stepResults,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        long durationMs,
        ExecutionError? testLevelError = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testId);
        ArgumentNullException.ThrowIfNull(stepResults);

        _logger.LogInformation("Aggregating test result for TestId={TestId}", testId);

        var blockedSteps = stepResults.Count(step => step.Status == StepStatus.Blocked);
        var failedSteps = stepResults.Count(step => step.Status == StepStatus.Failed);
        var passedSteps = stepResults.Count(step => step.Status == StepStatus.Passed);
        var skippedSteps = stepResults.Count(step => step.Status == StepStatus.Skipped);

        var status = blockedSteps > 0
            ? TestStatus.Blocked
            : failedSteps > 0
                ? TestStatus.Failed
                : TestStatus.Passed;

        if (testLevelError is not null)
        {
            status = TestStatus.Blocked;
        }

        return new TestExecutionResult
        {
            TestId = testId,
            Status = status,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            DurationMs = durationMs,
            TotalSteps = stepResults.Count,
            PassedSteps = passedSteps,
            FailedSteps = failedSteps,
            BlockedSteps = blockedSteps,
            SkippedSteps = skippedSteps,
            StepResults = stepResults.ToList(),
            Error = testLevelError
        };
    }
}
