using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugCreationService
{
    Task<BugCreationResult> CreateIfApprovedAsync(
        BugRecommendationResult recommendation,
        BugDraftHistoryEntry draftHistory,
        CancellationToken cancellationToken = default);
}
