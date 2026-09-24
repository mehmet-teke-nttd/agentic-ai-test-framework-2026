using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IExecutionStepValidator
{
    ExecutionStepValidationResult Validate(ExecutionStep executionStep);
}
