using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class ExecutionCommandHandlerResolver : IExecutionCommandHandlerResolver
{
    private readonly IReadOnlyDictionary<ExecutionKeyword, IExecutionCommandHandler> _handlers;

    public ExecutionCommandHandlerResolver(IEnumerable<IExecutionCommandHandler> handlers)
    {
        _handlers = handlers.ToDictionary(handler => handler.Keyword);
    }

    public IExecutionCommandHandler Resolve(ExecutionKeyword keyword)
    {
        if (_handlers.TryGetValue(keyword, out var handler))
        {
            return handler;
        }

        throw new InvalidOperationException($"No execution command handler registered for keyword '{keyword}'.");
    }
}
