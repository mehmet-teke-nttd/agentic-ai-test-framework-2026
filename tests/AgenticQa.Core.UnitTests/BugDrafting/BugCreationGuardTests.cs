using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugCreationGuardTests
{
    [Test]
    public void UnapprovedBug_Blocked()
    {
        var guard = new BugCreationGuard(NullLogger<BugCreationGuard>.Instance);
        var recommendation = new BugRecommendationResult
        {
            TestId = "TC-1",
            Status = BugRecommendationStatus.BugDraftRecommended,
            Reason = "r"
        };
        var history = CreateHistory(BugDraftStatus.Draft, BugDraftReviewDecision.Rejected);

        var result = guard.Validate(recommendation, history);
        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("BUG_NOT_APPROVED"));
    }

    [Test]
    public void ApprovedBug_GuardPasses()
    {
        var guard = new BugCreationGuard(NullLogger<BugCreationGuard>.Instance);
        var recommendation = new BugRecommendationResult
        {
            TestId = "TC-1",
            Status = BugRecommendationStatus.BugDraftRecommended,
            Reason = "r"
        };
        var history = CreateHistory(BugDraftStatus.Approved, BugDraftReviewDecision.Approved);

        var result = guard.Validate(recommendation, history);
        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.ReadyForCreation));
    }

    private static BugDraftHistoryEntry CreateHistory(BugDraftStatus status, BugDraftReviewDecision decision) =>
        new()
        {
            TestId = "TC-1",
            Recommendation = new BugRecommendationResult
            {
                TestId = "TC-1",
                Status = BugRecommendationStatus.BugDraftRecommended,
                Reason = "reason"
            },
            OriginalDraft = new BugDraft
            {
                TestId = "TC-1",
                Title = "title",
                Summary = "summary",
                ExpectedResult = "expected",
                ActualResult = "actual",
                Environment = "UI"
            },
            Status = status,
            Review = new BugDraftReview
            {
                TestId = "TC-1",
                Decision = decision,
                ReviewedBy = "QA",
                Comment = "comment",
                ReviewedAt = DateTimeOffset.UtcNow
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
}
