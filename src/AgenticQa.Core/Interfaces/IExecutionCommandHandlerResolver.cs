using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Interfaces;

public interface IExecutionCommandHandlerResolver
{
    IExecutionCommandHandler Resolve(ExecutionKeyword keyword);
}
