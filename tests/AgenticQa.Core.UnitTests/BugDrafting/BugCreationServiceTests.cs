using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugCreationServiceTests
{
    [Test]
    public async Task DuplicateBug_DoesNotCallCreateApi()
    {
        var guard = Substitute.For<IBugCreationGuard>();
        guard.Validate(Arg.Any<BugRecommendationResult>(), Arg.Any<BugDraftHistoryEntry>())
            .Returns(new BugCreationResult { TestId = "TC-1", Status = BugCreationStatus.ReadyForCreation });
        var duplicateChecker = Substitute.For<IBugDuplicateChecker>();
        duplicateChecker.FindDuplicateAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>())
            .Returns(new DuplicateBugMatch
            {
                IsDuplicate = true,
                ExistingBugId = "123",
                ExistingBugUrl = "https://dev.azure.com/bug/123",
                Reason = "existing duplicate"
            });
        var tracker = Substitute.For<IBugTrackerClient>();
        var persistence = Substitute.For<IBugCreationResultPersistenceService>();

        var service = new BugCreationService(
            NullLogger<BugCreationService>.Instance,
            guard,
            duplicateChecker,
            tracker,
            persistence);

        var result = await service.CreateIfApprovedAsync(CreateRecommendation(), CreateHistory());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Duplicate));
        await tracker.DidNotReceive().CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task NoDuplicate_CallsCreateApiOnce()
    {
        var guard = Substitute.For<IBugCreationGuard>();
        guard.Validate(Arg.Any<BugRecommendationResult>(), Arg.Any<BugDraftHistoryEntry>())
            .Returns(new BugCreationResult { TestId = "TC-1", Status = BugCreationStatus.ReadyForCreation });
        var duplicateChecker = Substitute.For<IBugDuplicateChecker>();
        duplicateChecker.FindDuplicateAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>())
            .Returns(new DuplicateBugMatch { IsDuplicate = false });
        var tracker = Substitute.For<IBugTrackerClient>();
        tracker.CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>())
            .Returns(new BugCreationResult
            {
                TestId = "TC-1",
                Status = BugCreationStatus.Created,
                ExternalId = "123"
            });
        var persistence = Substitute.For<IBugCreationResultPersistenceService>();

        var service = new BugCreationService(
            NullLogger<BugCreationService>.Instance,
            guard,
            duplicateChecker,
            tracker,
            persistence);

        var result = await service.CreateIfApprovedAsync(CreateRecommendation(), CreateHistory());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Created));
        await tracker.Received(1).CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task IdempotentResubmission_DoesNotCreateSecondBug()
    {
        var guard = Substitute.For<IBugCreationGuard>();
        guard.Validate(Arg.Any<BugRecommendationResult>(), Arg.Any<BugDraftHistoryEntry>())
            .Returns(new BugCreationResult { TestId = "TC-1", Status = BugCreationStatus.ReadyForCreation });
        var duplicateChecker = Substitute.For<IBugDuplicateChecker>();
        duplicateChecker.FindDuplicateAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>())
            .Returns(new DuplicateBugMatch { IsDuplicate = false });
        var tracker = Substitute.For<IBugTrackerClient>();
        var persistence = Substitute.For<IBugCreationResultPersistenceService>();
        persistence.LoadByCreationKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new BugCreationResult
            {
                TestId = "TC-1",
                Status = BugCreationStatus.Created,
                ExternalId = "123",
                CreationKey = "TC-1:key"
            });

        var service = new BugCreationService(
            NullLogger<BugCreationService>.Instance,
            guard,
            duplicateChecker,
            tracker,
            persistence);

        var result = await service.CreateIfApprovedAsync(CreateRecommendation(), CreateHistory());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Created));
        await tracker.DidNotReceive().CreateBugAsync(Arg.Any<ApprovedBugDraft>(), Arg.Any<CancellationToken>());
    }

    private static BugRecommendationResult CreateRecommendation() =>
        new()
        {
            TestId = "TC-1",
            Status = BugRecommendationStatus.BugDraftRecommended,
            Reason = "confirmed"
        };

    private static BugDraftHistoryEntry CreateHistory() =>
        new()
        {
            TestId = "TC-1",
            Recommendation = CreateRecommendation(),
            OriginalDraft = new BugDraft
            {
                TestId = "TC-1",
                Title = "title",
                Summary = "summary",
                ExpectedResult = "expected",
                ActualResult = "actual",
                Environment = "UI",
                FailureClassification = FailureClassification.ProductDefect
            },
            Status = BugDraftStatus.Approved,
            Review = new BugDraftReview
            {
                TestId = "TC-1",
                Decision = BugDraftReviewDecision.Approved,
                ReviewedBy = "QA",
                Comment = "ok",
                ReviewedAt = DateTimeOffset.Parse("2026-09-23T20:00:00Z")
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
}
