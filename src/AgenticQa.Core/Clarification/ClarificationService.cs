using System.Text.Json;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Clarification;

public sealed class ClarificationService : IClarificationService
{
    private readonly ILogger<ClarificationService> _logger;
    private readonly ITestIntentValidator _testIntentValidator;

    public ClarificationService(
        ILogger<ClarificationService> logger,
        ITestIntentValidator testIntentValidator)
    {
        _logger = logger;
        _testIntentValidator = testIntentValidator;
    }

    public ClarificationWorkflowResult ApplyClarification(
        TestIntent testIntent,
        TestIntentValidationResult currentValidation,
        ClarificationResponse clarificationResponse)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(currentValidation);
        ArgumentNullException.ThrowIfNull(clarificationResponse);

        var issue = currentValidation.Issues.FirstOrDefault(x => x.IssueId == clarificationResponse.IssueId);
        if (issue is null)
        {
            return Reject(
                testIntent,
                currentValidation,
                $"Unknown issueId '{clarificationResponse.IssueId}'.");
        }

        if (string.IsNullOrWhiteSpace(clarificationResponse.Response))
        {
            return Reject(
                testIntent,
                currentValidation,
                "Clarification response must not be empty.");
        }

        switch (clarificationResponse.Outcome)
        {
            case ClarificationOutcome.RejectedAssumption:
                AddHistory(testIntent, clarificationResponse);
                _logger.LogInformation(
                    "Assumption rejected for TestId={TestId}, IssueId={IssueId}",
                    testIntent.TestId,
                    clarificationResponse.IssueId);

                return new ClarificationWorkflowResult
                {
                    IsAccepted = true,
                    IsBlocked = true,
                    UpdatedIntent = testIntent,
                    ValidationResult = currentValidation
                };

            case ClarificationOutcome.Clarification:
            case ClarificationOutcome.ApprovedAssumption:
                if (!TryApplyFieldUpdate(testIntent, issue.Field, clarificationResponse.Response, out var rejectionReason))
                {
                    return Reject(testIntent, currentValidation, rejectionReason!);
                }

                AddHistory(testIntent, clarificationResponse);

                if (clarificationResponse.Outcome == ClarificationOutcome.ApprovedAssumption)
                {
                    testIntent.ApprovedAssumptions.Add(new ApprovedAssumption
                    {
                        IssueId = clarificationResponse.IssueId,
                        Field = issue.Field,
                        Value = clarificationResponse.Response,
                        ApprovedBy = clarificationResponse.RespondedBy
                    });

                    _logger.LogInformation(
                        "Assumption approved for TestId={TestId}, IssueId={IssueId}, Field={Field}",
                        testIntent.TestId,
                        clarificationResponse.IssueId,
                        issue.Field);
                }
                else
                {
                    _logger.LogInformation(
                        "Clarification applied for TestId={TestId}, IssueId={IssueId}, Field={Field}",
                        testIntent.TestId,
                        clarificationResponse.IssueId,
                        issue.Field);
                }

                var revalidation = _testIntentValidator.Validate(testIntent);
                return new ClarificationWorkflowResult
                {
                    IsAccepted = true,
                    IsBlocked = revalidation.Status != ValidationStatus.Valid,
                    UpdatedIntent = testIntent,
                    ValidationResult = revalidation
                };

            default:
                return Reject(
                    testIntent,
                    currentValidation,
                    $"Unsupported clarification outcome '{clarificationResponse.Outcome}'.");
        }
    }

    private void AddHistory(TestIntent testIntent, ClarificationResponse response)
    {
        testIntent.ClarificationHistory.Add(new ClarificationHistoryEntry
        {
            IssueId = response.IssueId,
            Outcome = response.Outcome,
            Response = response.Response,
            RespondedBy = response.RespondedBy
        });
    }

    private ClarificationWorkflowResult Reject(
        TestIntent testIntent,
        TestIntentValidationResult currentValidation,
        string reason)
    {
        _logger.LogWarning("Clarification rejected for TestId={TestId}: {Reason}", testIntent.TestId, reason);
        return new ClarificationWorkflowResult
        {
            IsAccepted = false,
            IsBlocked = true,
            RejectionReason = reason,
            UpdatedIntent = testIntent,
            ValidationResult = currentValidation
        };
    }

    private static bool TryApplyFieldUpdate(
        TestIntent testIntent,
        string field,
        string rawResponse,
        out string? rejectionReason)
    {
        rejectionReason = null;

        switch (field)
        {
            case "testId":
                if (LooksLikeStructuredPayload(rawResponse))
                {
                    rejectionReason = "Field 'testId' accepts only plain text clarification.";
                    return false;
                }

                testIntent.TestId = rawResponse;
                return true;
            case "title":
                if (LooksLikeStructuredPayload(rawResponse))
                {
                    rejectionReason = "Field 'title' accepts only plain text clarification.";
                    return false;
                }

                testIntent.Title = rawResponse;
                return true;
            case "preconditions":
                return TryApplyStringList(rawResponse, value => testIntent.Preconditions = value, field, out rejectionReason);
            case "expectedResults":
                return TryApplyStringList(rawResponse, value => testIntent.ExpectedResults = value, field, out rejectionReason);
            case "actions":
                return TryApplyActions(rawResponse, testIntent, out rejectionReason);
            case "testData":
                return TryApplyTestDataObject(rawResponse, testIntent, out rejectionReason);
            default:
                if (field.StartsWith("testData.", StringComparison.Ordinal))
                {
                    var key = field["testData.".Length..];
                    testIntent.TestData ??= [];
                    testIntent.TestData[key] = rawResponse;
                    return true;
                }

                rejectionReason = $"Field '{field}' is not supported for clarification updates.";
                return false;
        }
    }

    private static bool TryApplyStringList(
        string rawResponse,
        Action<List<string>> assign,
        string field,
        out string? rejectionReason)
    {
        rejectionReason = null;
        try
        {
            List<string> values;
            if (rawResponse.TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                values = AgenticJsonSerializer.Deserialize<List<string>>(rawResponse) ?? [];
            }
            else if (rawResponse.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                rejectionReason = $"Invalid value for field '{field}'. Unexpected object payload.";
                return false;
            }
            else
            {
                values = rawResponse
                    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
            }

            assign(values);
            return true;
        }
        catch (JsonException)
        {
            rejectionReason = $"Invalid value for field '{field}'. Expected a JSON array of strings.";
            return false;
        }
    }

    private static bool TryApplyActions(string rawResponse, TestIntent testIntent, out string? rejectionReason)
    {
        rejectionReason = null;
        try
        {
            var actions = AgenticJsonSerializer.Deserialize<List<TestAction>>(rawResponse);
            if (actions is null)
            {
                rejectionReason = "Invalid value for field 'actions'.";
                return false;
            }

            testIntent.Actions = actions;
            return true;
        }
        catch (JsonException)
        {
            rejectionReason = "Invalid value for field 'actions'. Expected JSON array of action objects.";
            return false;
        }
    }

    private static bool TryApplyTestDataObject(string rawResponse, TestIntent testIntent, out string? rejectionReason)
    {
        rejectionReason = null;
        try
        {
            var data = AgenticJsonSerializer.Deserialize<Dictionary<string, object?>>(rawResponse);
            if (data is null)
            {
                rejectionReason = "Invalid value for field 'testData'.";
                return false;
            }

            testIntent.TestData = data;
            return true;
        }
        catch (JsonException)
        {
            rejectionReason = "Invalid value for field 'testData'. Expected JSON object.";
            return false;
        }
    }

    private static bool LooksLikeStructuredPayload(string rawResponse)
    {
        var trimmed = rawResponse.TrimStart();
        return trimmed.StartsWith("{", StringComparison.Ordinal)
            || trimmed.StartsWith("[", StringComparison.Ordinal);
    }
}
