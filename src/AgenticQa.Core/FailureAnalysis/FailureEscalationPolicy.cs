using AgenticQa.Core.Enums;

namespace AgenticQa.Core.FailureAnalysis;

public static class FailureEscalationPolicy
{
    public static (bool EscalationRequired, FailureEscalationLevel EscalationLevel, string EscalationReason) Evaluate(
        FailureClassification classification,
        double confidence,
        FailureRecommendedAction recommendedAction)
    {
        if (classification == FailureClassification.ProductDefect && confidence >= 0.80)
        {
            return (true, FailureEscalationLevel.High, "High-confidence product defect should be escalated for triage.");
        }

        if (classification == FailureClassification.Unknown)
        {
            return (true, FailureEscalationLevel.Medium, "Unknown classification requires manual QA triage.");
        }

        if (recommendedAction == FailureRecommendedAction.QaReviewRequired && confidence < 0.60)
        {
            return (true, FailureEscalationLevel.Medium, "Low-confidence recommendation needs human validation.");
        }

        if (classification == FailureClassification.EnvironmentIssue && confidence >= 0.85)
        {
            return (true, FailureEscalationLevel.Medium, "High-confidence environment issue may impact broader test stability.");
        }

        return (false, FailureEscalationLevel.None, "No escalation required.");
    }
}
