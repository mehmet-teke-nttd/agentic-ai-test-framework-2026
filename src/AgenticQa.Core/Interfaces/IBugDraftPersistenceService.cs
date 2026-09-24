using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugDraftPersistenceService
{
    Task<string> SaveDraftAsync(
        BugRecommendationResult recommendation,
        BugDraft originalDraft,
        BugDraft? aiPolishedDraft = null,
        CancellationToken cancellationToken = default);

    Task<string> SaveReviewAsync(
        string testId,
        BugDraftReviewDecision decision,
        string reviewedBy,
        string comment,
        BugDraft? reviewedDraft = null,
        CancellationToken cancellationToken = default);

    Task<BugDraftHistoryEntry?> LoadAsync(string testId, CancellationToken cancellationToken = default);
}
