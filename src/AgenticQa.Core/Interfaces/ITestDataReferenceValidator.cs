using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestDataReferenceValidator
{
    IReadOnlyList<ExecutionContractValidationError> Validate(
        ExecutionStep executionStep,
        IReadOnlyDictionary<string, object?> testData);
}
