using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IExecutionContractMapper
{
    ExecutionContractMappingResult Map(TestIntent testIntent);
}
