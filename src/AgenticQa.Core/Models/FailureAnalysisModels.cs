using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class FailureAnalysisRequest
{
    public string TestId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public TestType TestType { get; init; }
    public IReadOnlyList<string> ExpectedResults { get; init; } = [];
    public FailureAnalysisStepSnapshot? FailedStep { get; init; }
    public IReadOnlyList<FailureAnalysisStepSnapshot> PreviousStepResults { get; init; } = [];
    public string? CurrentPage { get; init; }
    public string? ScreenshotPath { get; init; }
    public IReadOnlyList<string> RelevantLogs { get; init; } = [];
}

public sealed class FailureAnalysisStepSnapshot
{
    public int Step { get; init; }
    public ExecutionKeyword Keyword { get; init; }
    public string? Target { get; init; }
    public StepStatus StepStatus { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class FailureAnalysisResult
{
    public string TestId { get; init; } = string.Empty;
    public FailureClassification Classification { get; init; }
    public double Confidence { get; init; }
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> Evidence { get; init; } = [];
    public FailureRecommendedAction RecommendedAction { get; init; }
    public bool EscalationRequired { get; init; }
    public FailureEscalationLevel EscalationLevel { get; init; } = FailureEscalationLevel.None;
    public string EscalationReason { get; init; } = string.Empty;
}

public sealed class FailureReviewRecord
{
    public string TestId { get; init; } = string.Empty;
    public FailureClassification AgentClassification { get; init; }
    public FailureReviewDecision QaDecision { get; init; }
    public string ReviewedBy { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; init; }
}

public sealed class FailureAnalysisHistoryEntry
{
    public string TestId { get; init; } = string.Empty;
    public FailureAnalysisResult AnalysisResult { get; init; } = new()
    {
        TestId = string.Empty,
        Classification = FailureClassification.Unknown,
        Confidence = 0.0,
        Summary = string.Empty,
        Evidence = [],
        RecommendedAction = FailureRecommendedAction.QaReviewRequired,
        EscalationRequired = true,
        EscalationLevel = FailureEscalationLevel.Medium,
        EscalationReason = "No analysis data was recorded."
    };

    public DateTimeOffset AnalyzedAt { get; init; }
    public FailureReviewRecord? QaReview { get; init; }
}

public sealed class FailureAnalysisPrompt
{
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
}
