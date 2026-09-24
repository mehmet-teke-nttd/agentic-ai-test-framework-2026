using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class BugRecommendationResult
{
    public string TestId { get; init; } = string.Empty;
    public BugRecommendationStatus Status { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed class BugDraft
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> StepsToReproduce { get; set; } = [];
    public string ExpectedResult { get; set; } = string.Empty;
    public string ActualResult { get; set; } = string.Empty;
    public BugSeverity Severity { get; set; } = BugSeverity.Medium;
    public BugPriority Priority { get; set; } = BugPriority.Unassigned;
    public string Environment { get; set; } = string.Empty;
    public string TestId { get; set; } = string.Empty;
    public int? FailedStep { get; set; }
    public List<string> Evidence { get; set; } = [];
    public List<string> Attachments { get; set; } = [];
    public List<string> RelatedAcceptanceCriteria { get; set; } = [];
    public FailureClassification FailureClassification { get; set; }
    public double FailureConfidence { get; set; }
}

public sealed class BugDraftRequest
{
    public string TestId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> Actions { get; init; } = [];
    public IReadOnlyList<string> ExpectedResults { get; init; } = [];
    public int? FailedStep { get; init; }
    public string? ActualError { get; init; }
    public FailureClassification FailureClassification { get; init; }
    public FailureReviewDecision FailureReviewDecision { get; init; }
    public string EnvironmentMetadata { get; init; } = string.Empty;
    public IReadOnlyList<string> EvidenceReferences { get; init; } = [];
}

public sealed class BugDraftPolishResult
{
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string ActualResult { get; init; } = string.Empty;
}

public sealed class BugDraftReview
{
    public string TestId { get; init; } = string.Empty;
    public BugDraftReviewDecision Decision { get; init; }
    public string ReviewedBy { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; init; }
}

public sealed class BugDraftHistoryEntry
{
    public string TestId { get; init; } = string.Empty;
    public BugRecommendationResult Recommendation { get; init; } = new()
    {
        TestId = string.Empty,
        Status = BugRecommendationStatus.NoBug,
        Reason = string.Empty
    };
    public BugDraft OriginalDraft { get; init; } = new();
    public BugDraft? AiPolishedDraft { get; init; }
    public BugDraft? QaReviewedDraft { get; init; }
    public BugDraftStatus Status { get; init; } = BugDraftStatus.Draft;
    public BugDraftReview? Review { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ApprovedBugDraft
{
    public string TestId { get; init; } = string.Empty;
    public string CreationKey { get; init; } = string.Empty;
    public BugDraft Draft { get; init; } = new();
    public BugDraftReview Review { get; init; } = new()
    {
        TestId = string.Empty,
        Decision = BugDraftReviewDecision.Rejected,
        ReviewedBy = string.Empty,
        Comment = string.Empty,
        ReviewedAt = DateTimeOffset.MinValue
    };
    public FailureClassification FailureClassification { get; init; }
}

public sealed class BugCreationResult
{
    public string TestId { get; init; } = string.Empty;
    public BugCreationStatus Status { get; init; }
    public string? ExternalId { get; init; }
    public string? ExternalUrl { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public ExecutionError? Error { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? CreationKey { get; init; }
    public string? ExistingBugId { get; init; }
    public string? ExistingBugUrl { get; init; }
}

public sealed record BugCreationResultHistory
{
    public string TestId { get; init; } = string.Empty;
    public ApprovedBugDraft? ApprovedDraft { get; init; }
    public DuplicateBugMatch? DuplicateCheck { get; init; }
    public BugCreationResult? LatestResult { get; init; }
    public IReadOnlyList<BugCreationResult> Results { get; init; } = [];
}
