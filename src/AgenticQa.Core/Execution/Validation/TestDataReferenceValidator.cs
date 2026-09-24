using System.Text.RegularExpressions;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Validation;

public sealed partial class TestDataReferenceValidator : ITestDataReferenceValidator
{
    private readonly ILogger<TestDataReferenceValidator> _logger;

    public TestDataReferenceValidator(ILogger<TestDataReferenceValidator> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<ExecutionContractValidationError> Validate(
        ExecutionStep executionStep,
        IReadOnlyDictionary<string, object?> testData)
    {
        ArgumentNullException.ThrowIfNull(executionStep);
        ArgumentNullException.ThrowIfNull(testData);

        var errors = new List<ExecutionContractValidationError>();
        ValidateField(executionStep, executionStep.Value, "value", testData, errors);
        ValidateField(executionStep, executionStep.Expected, "expected", testData, errors);

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Test data reference validation failed for Step={Step}, Keyword={Keyword}, ErrorCount={ErrorCount}",
                executionStep.Step,
                executionStep.Keyword,
                errors.Count);
        }

        return errors;
    }

    private static void ValidateField(
        ExecutionStep executionStep,
        string? content,
        string fieldName,
        IReadOnlyDictionary<string, object?> testData,
        ICollection<ExecutionContractValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var matches = PlaceholderRegex().Matches(content);
        foreach (Match match in matches)
        {
            var variableName = match.Groups["name"].Value;
            if (!testData.ContainsKey(variableName))
            {
                errors.Add(new ExecutionContractValidationError
                {
                    ErrorCode = ExecutionContractValidationErrorCode.TestDataReferenceNotFound,
                    Step = executionStep.Step,
                    Message = $"Test data reference '{{{{{variableName}}}}}' in {fieldName} is not defined."
                });
            }
        }
    }

    [GeneratedRegex(@"\{\{\s*(?<name>[a-zA-Z0-9_]+)\s*\}\}")]
    private static partial Regex PlaceholderRegex();
}
