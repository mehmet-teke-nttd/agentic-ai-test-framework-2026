using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IExecutionContractValidator
{
    ExecutionContractValidationResult Validate(ExecutionContract executionContract, TestIntent testIntent);
}
