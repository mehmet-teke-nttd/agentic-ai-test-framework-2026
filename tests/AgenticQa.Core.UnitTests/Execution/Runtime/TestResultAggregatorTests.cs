using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class TestResultAggregatorTests
{
    private TestResultAggregator _aggregator = null!;

    [SetUp]
    public void Setup()
    {
        _aggregator = new TestResultAggregator(NullLogger<TestResultAggregator>.Instance);
    }

    [Test]
    public void AllPassed_ReturnsPassed()
    {
        var result = Aggregate(
            Step(StepStatus.Passed, 1),
            Step(StepStatus.Passed, 2));

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }

    [Test]
    public void OneFailed_ReturnsFailed()
    {
        var result = Aggregate(
            Step(StepStatus.Passed, 1),
            Step(StepStatus.Failed, 2),
            Step(StepStatus.Skipped, 3));

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
    }

    [Test]
    public void OneBlocked_ReturnsBlocked()
    {
        var result = Aggregate(
            Step(StepStatus.Passed, 1),
            Step(StepStatus.Blocked, 2),
            Step(StepStatus.Skipped, 3));

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
    }

    [Test]
    public void BlockedTakesPrecedenceOverFailed()
    {
        var result = Aggregate(
            Step(StepStatus.Passed, 1),
            Step(StepStatus.Failed, 2),
            Step(StepStatus.Blocked, 3));

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
    }

    private TestExecutionResult Aggregate(params StepExecutionResult[] steps) =>
        _aggregator.Aggregate(
            "TC-001",
            steps,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddSeconds(1),
            1000);

    private static StepExecutionResult Step(StepStatus status, int step) =>
        new()
        {
            TestId = "TC-001",
            Step = step,
            Keyword = ExecutionKeyword.Click,
            Status = status,
            DurationMs = 10
        };
}
