using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class StepExecutorTests
{
    [Test]
    public async Task UnsupportedHandler_ReturnsBlocked()
    {
        var resolver = Substitute.For<IExecutionCommandHandlerResolver>();
        resolver.Resolve(ExecutionKeyword.Click).Returns(_ => throw new InvalidOperationException("missing"));
        var executor = new StepExecutor(NullLogger<StepExecutor>.Instance, resolver);

        var result = await executor.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" },
            CreateContext());

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("UNSUPPORTED_HANDLER"));
        Assert.That(result.DurationMs, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task SuccessfulExecution_StampsTimingAndMetadata()
    {
        var handler = new StubHandler(ExecutionKeyword.Click, StepStatus.Passed);
        var resolver = Substitute.For<IExecutionCommandHandlerResolver>();
        resolver.Resolve(ExecutionKeyword.Click).Returns(handler);
        var executor = new StepExecutor(NullLogger<StepExecutor>.Instance, resolver);

        var step = new ExecutionStep { Step = 2, Keyword = ExecutionKeyword.Click, Target = "LoginButton" };
        var context = CreateContext();
        var result = await executor.ExecuteAsync(step, context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
        Assert.That(result.Step, Is.EqualTo(2));
        Assert.That(result.TestId, Is.EqualTo(context.TestIntent.TestId));
        Assert.That(result.StartedAt, Is.Not.EqualTo(default(DateTimeOffset)));
        Assert.That(result.DurationMs, Is.GreaterThanOrEqualTo(0));
    }

    private static ExecutionContext CreateContext() =>
        new()
        {
            Page = Substitute.For<IPage>(),
            CurrentPage = "LoginPage",
            TestIntent = new TestIntent
            {
                TestId = "TC-001",
                Title = "Login",
                TestType = TestType.UiFunctionalPositive,
                Preconditions = ["precondition"],
                TestData = new Dictionary<string, object?>(),
                Actions = [new TestAction { Step = 1, Action = "Click Login button" }],
                ExpectedResults = ["success"]
            }
        };

    private sealed class StubHandler : IExecutionCommandHandler
    {
        private readonly StepStatus _status;

        public StubHandler(ExecutionKeyword keyword, StepStatus status)
        {
            Keyword = keyword;
            _status = status;
        }

        public ExecutionKeyword Keyword { get; }

        public Task<StepExecutionResult> ExecuteAsync(
            ExecutionStep step,
            ExecutionContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StepExecutionResult
            {
                Status = _status
            });
    }
}
