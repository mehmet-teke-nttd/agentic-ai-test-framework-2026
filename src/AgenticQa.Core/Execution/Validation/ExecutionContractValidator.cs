using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Validation;

public sealed class ExecutionContractValidator : IExecutionContractValidator
{
    private readonly ILogger<ExecutionContractValidator> _logger;
    private readonly IExecutionStepValidator _executionStepValidator;
    private readonly ITestDataReferenceValidator _testDataReferenceValidator;

    public ExecutionContractValidator(
        ILogger<ExecutionContractValidator> logger,
        IExecutionStepValidator executionStepValidator,
        ITestDataReferenceValidator testDataReferenceValidator)
    {
        _logger = logger;
        _executionStepValidator = executionStepValidator;
        _testDataReferenceValidator = testDataReferenceValidator;
    }

    public ExecutionContractValidationResult Validate(ExecutionContract executionContract, TestIntent testIntent)
    {
        ArgumentNullException.ThrowIfNull(executionContract);
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(testIntent.TestData);
        ContractSchemaMigration.Normalize(executionContract);
        ContractSchemaMigration.Normalize(testIntent);

        _logger.LogInformation("Execution contract validation started for TestId={TestId}", executionContract.TestId);

        var errors = new List<ExecutionContractValidationError>();

        if (!ContractSchemaVersions.IsSupported(executionContract.SchemaVersion))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.UnsupportedSchemaVersion,
                null,
                $"Execution contract schemaVersion '{executionContract.SchemaVersion}' is not supported."));
        }

        if (string.IsNullOrWhiteSpace(executionContract.TestId))
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.MissingTestId,
                null,
                "Execution contract testId is required."));
        }

        if (executionContract.Steps is null || executionContract.Steps.Count == 0)
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.NoExecutionSteps,
                null,
                "Execution contract must contain at least one step."));

            _logger.LogInformation(
                "Execution contract validation completed for TestId={TestId}. IsValid={IsValid}, ErrorCount={ErrorCount}",
                executionContract.TestId,
                false,
                errors.Count);

            return new ExecutionContractValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }

        var orderedSteps = executionContract.Steps
            .Select(step => step.Step)
            .OrderBy(step => step)
            .ToList();

        if (orderedSteps[0] != 1)
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.NonSequentialSteps,
                orderedSteps[0],
                "Execution step numbering must start at 1."));
        }

        var duplicateStepNumbers = orderedSteps
            .GroupBy(step => step)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateStep in duplicateStepNumbers)
        {
            errors.Add(CreateError(
                ExecutionContractValidationErrorCode.DuplicateStepNumber,
                duplicateStep,
                $"Duplicate execution step number detected: {duplicateStep}."));
        }

        for (var expected = 1; expected <= orderedSteps.Count; expected++)
        {
            if (orderedSteps[expected - 1] != expected)
            {
                errors.Add(CreateError(
                    ExecutionContractValidationErrorCode.NonSequentialSteps,
                    orderedSteps[expected - 1],
                    "Execution step numbers must be sequential (1, 2, 3...)."));
                break;
            }
        }

        foreach (var step in executionContract.Steps)
        {
            var stepResult = _executionStepValidator.Validate(step);
            if (!stepResult.IsValid)
            {
                errors.AddRange(stepResult.Errors);
            }

            var referenceErrors = _testDataReferenceValidator.Validate(step, testIntent.TestData);
            errors.AddRange(referenceErrors);
        }

        _logger.LogInformation(
            "Execution contract validation completed for TestId={TestId}. IsValid={IsValid}, ErrorCount={ErrorCount}",
            executionContract.TestId,
            errors.Count == 0,
            errors.Count);

        return new ExecutionContractValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
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
