using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugCreationGuard
{
    BugCreationResult Validate(
        BugRecommendationResult recommendation,
        BugDraftHistoryEntry draftHistory);
}
