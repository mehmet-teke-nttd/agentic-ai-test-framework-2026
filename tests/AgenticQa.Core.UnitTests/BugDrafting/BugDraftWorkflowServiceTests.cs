using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugDraftWorkflowServiceTests
{
    [Test]
    public async Task RejectedDraft_CannotBeSentToTracker()
    {
        var tracker = Substitute.For<IBugTrackerClient>();
        var service = new BugDraftWorkflowService(NullLogger<BugDraftWorkflowService>.Instance, tracker);

        var history = CreateHistory(BugDraftStatus.Rejected, BugDraftReviewDecision.Rejected);
        var result = await service.CreateBugIfApprovedAsync(history);

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Blocked));
        await tracker.DidNotReceive().CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UnapprovedDraft_CannotBeSentToTracker()
    {
        var tracker = Substitute.For<IBugTrackerClient>();
        var service = new BugDraftWorkflowService(NullLogger<BugDraftWorkflowService>.Instance, tracker);

        var history = CreateHistory(BugDraftStatus.Draft, BugDraftReviewDecision.Rejected);
        var result = await service.CreateBugIfApprovedAsync(history);

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Blocked));
        await tracker.DidNotReceive().CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ApprovedDraft_CanBeSentToTracker()
    {
        var tracker = Substitute.For<IBugTrackerClient>();
        tracker.CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>())
            .Returns(new BugCreationResult { Status = BugCreationStatus.Blocked, Message = "disabled" });

        var service = new BugDraftWorkflowService(NullLogger<BugDraftWorkflowService>.Instance, tracker);
        var history = CreateHistory(BugDraftStatus.Approved, BugDraftReviewDecision.Approved);
        await service.CreateBugIfApprovedAsync(history);

        await tracker.Received(1).CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
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
