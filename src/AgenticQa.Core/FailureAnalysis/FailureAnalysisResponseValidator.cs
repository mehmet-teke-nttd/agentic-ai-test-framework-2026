using System.Text.Json;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class FailureAnalysisResponseValidator : IFailureAnalysisResponseValidator
{
    public bool TryParse(
        string testId,
        string aiResponse,
        out FailureAnalysisResult result,
        out string validationError)
    {
        result = UnknownResult(testId, "AI response validation failed.");
        validationError = string.Empty;

        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            validationError = "AI response is empty.";
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(aiResponse);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                validationError = "AI response must be a JSON object.";
                return false;
            }

            if (!TryReadString(root, "classification", out var classificationText))
            {
                validationError = "classification is required.";
                return false;
            }

            if (!TryParseClassification(classificationText, out var classification))
            {
                validationError = "classification value is unsupported.";
                return false;
            }

            if (!TryReadDouble(root, "confidence", out var confidence))
            {
                validationError = "confidence is required.";
                return false;
            }

            if (confidence < 0.0 || confidence > 1.0)
            {
                validationError = "confidence must be between 0 and 1.";
                return false;
            }

            if (!TryReadString(root, "summary", out var summary) || string.IsNullOrWhiteSpace(summary))
            {
                validationError = "summary is required.";
                return false;
            }

            if (!TryReadString(root, "recommendedAction", out var actionText))
            {
                validationError = "recommendedAction is required.";
                return false;
            }

            if (!TryParseRecommendedAction(actionText, out var recommendedAction))
            {
                validationError = "recommendedAction value is unsupported.";
                return false;
            }

            var escalation = FailureEscalationPolicy.Evaluate(classification, confidence, recommendedAction);
            var escalationRequired = escalation.EscalationRequired;
            var escalationLevel = escalation.EscalationLevel;
            var escalationReason = escalation.EscalationReason;

            if (root.TryGetProperty("escalationRequired", out var escalationRequiredElement))
            {
                if (escalationRequiredElement.ValueKind != JsonValueKind.True
                    && escalationRequiredElement.ValueKind != JsonValueKind.False)
                {
                    validationError = "escalationRequired must be a boolean.";
                    return false;
                }

                escalationRequired = escalationRequiredElement.GetBoolean();
            }

            if (TryReadString(root, "escalationLevel", out var escalationLevelText)
                && !string.IsNullOrWhiteSpace(escalationLevelText))
            {
                if (!TryParseEscalationLevel(escalationLevelText, out escalationLevel))
                {
                    validationError = "escalationLevel value is unsupported.";
                    return false;
                }
            }

            if (TryReadString(root, "escalationReason", out var escalationReasonText)
                && !string.IsNullOrWhiteSpace(escalationReasonText))
            {
                escalationReason = escalationReasonText;
            }

            if (!root.TryGetProperty("evidence", out var evidenceElement) || evidenceElement.ValueKind != JsonValueKind.Array)
            {
                validationError = "evidence must be an array.";
                return false;
            }

            var evidence = new List<string>();
            foreach (var item in evidenceElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    validationError = "evidence items must be strings.";
                    return false;
                }
                evidence.Add(item.GetString() ?? string.Empty);
            }

            result = new FailureAnalysisResult
            {
                TestId = testId,
                Classification = classification,
                Confidence = confidence,
                Summary = summary,
                Evidence = evidence,
                RecommendedAction = recommendedAction,
                EscalationRequired = escalationRequired,
                EscalationLevel = escalationLevel,
                EscalationReason = escalationReason
            };

            return true;
        }
        catch (JsonException ex)
        {
            validationError = $"AI response is not valid JSON: {ex.Message}";
            return false;
        }
    }

    public static FailureAnalysisResult UnknownResult(string testId, string reason) =>
        new()
        {
            TestId = testId,
            Classification = FailureClassification.Unknown,
            Confidence = 0.0,
            Summary = reason,
            Evidence = [reason],
            RecommendedAction = FailureRecommendedAction.QaReviewRequired,
            EscalationRequired = true,
            EscalationLevel = FailureEscalationLevel.Medium,
            EscalationReason = reason
        };

    private static bool TryReadString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString() ?? string.Empty;
        return true;
    }

    private static bool TryReadDouble(JsonElement root, string propertyName, out double value)
    {
        value = default;
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        return element.TryGetDouble(out value);
    }

    private static bool TryParseClassification(string text, out FailureClassification classification)
    {
        classification = text switch
        {
            "PRODUCT_DEFECT" => FailureClassification.ProductDefect,
            "AUTOMATION_ISSUE" => FailureClassification.AutomationIssue,
            "TEST_DATA_ISSUE" => FailureClassification.TestDataIssue,
            "ENVIRONMENT_ISSUE" => FailureClassification.EnvironmentIssue,
            "REQUIREMENT_ISSUE" => FailureClassification.RequirementIssue,
            "UNKNOWN" => FailureClassification.Unknown,
            _ => default
        };

        return text is "PRODUCT_DEFECT"
            or "AUTOMATION_ISSUE"
            or "TEST_DATA_ISSUE"
            or "ENVIRONMENT_ISSUE"
            or "REQUIREMENT_ISSUE"
            or "UNKNOWN";
    }

    private static bool TryParseRecommendedAction(string text, out FailureRecommendedAction action)
    {
        action = text switch
        {
            "QA_REVIEW_REQUIRED" => FailureRecommendedAction.QaReviewRequired,
            "INVESTIGATE_AUTOMATION" => FailureRecommendedAction.InvestigateAutomation,
            "CHECK_TEST_DATA" => FailureRecommendedAction.CheckTestData,
            "CHECK_ENVIRONMENT" => FailureRecommendedAction.CheckEnvironment,
            "REVIEW_REQUIREMENTS" => FailureRecommendedAction.ReviewRequirements,
            "NO_ACTION" => FailureRecommendedAction.NoAction,
            _ => default
        };

        return text is "QA_REVIEW_REQUIRED"
            or "INVESTIGATE_AUTOMATION"
            or "CHECK_TEST_DATA"
            or "CHECK_ENVIRONMENT"
            or "REVIEW_REQUIREMENTS"
            or "NO_ACTION";
    }

    private static bool TryParseEscalationLevel(string text, out FailureEscalationLevel escalationLevel)
    {
        escalationLevel = text switch
        {
            "NONE" => FailureEscalationLevel.None,
            "LOW" => FailureEscalationLevel.Low,
            "MEDIUM" => FailureEscalationLevel.Medium,
            "HIGH" => FailureEscalationLevel.High,
            "CRITICAL" => FailureEscalationLevel.Critical,
            _ => default
        };

        return text is "NONE" or "LOW" or "MEDIUM" or "HIGH" or "CRITICAL";
    }
}
