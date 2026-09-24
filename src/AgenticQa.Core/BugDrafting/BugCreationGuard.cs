using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugCreationGuard : IBugCreationGuard
{
    private readonly ILogger<BugCreationGuard> _logger;

    public BugCreationGuard(ILogger<BugCreationGuard> logger)
    {
        _logger = logger;
    }

    public BugCreationResult Validate(
        BugRecommendationResult recommendation,
        BugDraftHistoryEntry draftHistory)
    {
        if (recommendation.Status != BugRecommendationStatus.BugDraftRecommended)
        {
            return Blocked(draftHistory.TestId, "Bug recommendation status is not BUG_DRAFT_RECOMMENDED.");
        }

        if (draftHistory.Status != BugDraftStatus.Approved)
        {
            return Blocked(draftHistory.TestId, "Bug draft has not been approved by QA.");
        }

        if (draftHistory.Review is null
            || draftHistory.Review.Decision is not (BugDraftReviewDecision.Approved or BugDraftReviewDecision.EditedAndApproved))
        {
            return Blocked(draftHistory.TestId, "Bug draft review decision does not allow creation.");
        }

        _logger.LogInformation("Approval guard passed for TestId={TestId}", draftHistory.TestId);
        return new BugCreationResult
        {
            TestId = draftHistory.TestId,
            Status = BugCreationStatus.ReadyForCreation,
            Message = "Bug draft is approved for creation."
        };
    }

    private static BugCreationResult Blocked(string testId, string message) =>
        new()
        {
            TestId = testId,
            Status = BugCreationStatus.Blocked,
            Message = message,
            Error = new ExecutionError
            {
                ErrorCode = "BUG_NOT_APPROVED",
                Message = message
            }
        };
}
