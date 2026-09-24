using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Execution.Validation;

public sealed class ExecutionStepValidator : IExecutionStepValidator
{
    public ExecutionStepValidationResult Validate(ExecutionStep executionStep)
    {
        ArgumentNullException.ThrowIfNull(executionStep);

        var errors = new List<ExecutionContractValidationError>();

        if (executionStep.Step <= 0)
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.InvalidStepNumber,
                executionStep.Step,
                "Step number must be greater than zero."));
        }

        if (!Enum.IsDefined(executionStep.Keyword))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.UnsupportedKeyword,
                executionStep.Step,
                "Execution keyword is not supported."));
        }
        else
        {
            ValidateKeywordFields(executionStep, errors);
        }

        return new ExecutionStepValidationResult
        {
            IsValid = errors.Count == 0,
            Step = executionStep.Step,
            Errors = errors
        };
    }

    private static void ValidateKeywordFields(
        ExecutionStep executionStep,
        ICollection<ExecutionContractValidationError> errors)
    {
        switch (executionStep.Keyword)
        {
            case ExecutionKeyword.Navigate:
            case ExecutionKeyword.Click:
            case ExecutionKeyword.Check:
            case ExecutionKeyword.VerifyVisible:
                RequireTarget(executionStep, errors);
                break;

            case ExecutionKeyword.Fill:
            case ExecutionKeyword.Select:
                RequireTarget(executionStep, errors);
                RequireValue(executionStep, errors);
                break;

            case ExecutionKeyword.VerifyText:
                RequireTarget(executionStep, errors);
                RequireExpected(executionStep, errors);
                break;

            case ExecutionKeyword.VerifyUrl:
                RequireExpected(executionStep, errors);
                break;
        }
    }

    private static void RequireTarget(
        ExecutionStep executionStep,
        ICollection<ExecutionContractValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(executionStep.Target))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.MissingTarget,
                executionStep.Step,
                $"{executionStep.Keyword} requires a target."));
        }
    }

    private static void RequireValue(
        ExecutionStep executionStep,
        ICollection<ExecutionContractValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(executionStep.Value))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.MissingValue,
                executionStep.Step,
                $"{executionStep.Keyword} requires a value."));
        }
    }

    private static void RequireExpected(
        ExecutionStep executionStep,
        ICollection<ExecutionContractValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(executionStep.Expected))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.MissingExpected,
                executionStep.Step,
                $"{executionStep.Keyword} requires an expected value."));
        }
    }

    private static ExecutionContractValidationError CreateError(
        ExecutionContractValidationErrorCode errorCode,
        int? step,
        string message) =>
        new()
        {
            ErrorCode = errorCode,
            Step = step,
            Message = message
        };
}
