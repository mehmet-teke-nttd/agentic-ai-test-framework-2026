using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class ExecutionCommandHandlerResolverTests
{
    [Test]
    public void Resolve_KnownKeyword_ReturnsHandler()
    {
        var handlers = new[] { new StubHandler(ExecutionKeyword.Click) };
        var resolver = new ExecutionCommandHandlerResolver(handlers);

        var handler = resolver.Resolve(ExecutionKeyword.Click);

        Assert.That(handler.Keyword, Is.EqualTo(ExecutionKeyword.Click));
    }

    [Test]
    public void Resolve_UnknownKeyword_Throws()
    {
        var handlers = new[] { new StubHandler(ExecutionKeyword.Click) };
        var resolver = new ExecutionCommandHandlerResolver(handlers);

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve(ExecutionKeyword.Fill));
    }

    private sealed class StubHandler : IExecutionCommandHandler
    {
        public StubHandler(ExecutionKeyword keyword)
        {
            Keyword = keyword;
        }

        public ExecutionKeyword Keyword { get; }

        public Task<StepExecutionResult> ExecuteAsync(
            ExecutionStep step,
            ExecutionContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Status = StepStatus.Passed
            });
    }
}
