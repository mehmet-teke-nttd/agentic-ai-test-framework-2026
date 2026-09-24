using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugCreationResultPersistenceServiceTests
{
    [Test]
    public async Task SaveAndLoadByCreationKey_Works()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"agentic-bug-results-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDir);

        try
        {
            var service = new BugCreationResultPersistenceService(outputDir);
            var approved = new ApprovedBugDraft
            {
                TestId = "TC-1",
                CreationKey = "TC-1:key",
                FailureClassification = FailureClassification.ProductDefect,
                Review = new BugDraftReview
                {
                    TestId = "TC-1",
                    Decision = BugDraftReviewDecision.Approved,
                    ReviewedBy = "QA",
                    Comment = "ok",
                    ReviewedAt = DateTimeOffset.UtcNow
                },
                Draft = new BugDraft
                {
                    TestId = "TC-1",
                    Title = "title",
                    Summary = "summary",
                    ExpectedResult = "expected",
                    ActualResult = "actual",
                    Environment = "UI"
                }
            };
            var creation = new BugCreationResult
            {
                TestId = "TC-1",
                CreationKey = "TC-1:key",
                Status = BugCreationStatus.ReadyForCreation,
                Message = "dry run"
            };

            await service.SaveAsync(approved, creation);
            var loaded = await service.LoadByCreationKeyAsync("TC-1:key");

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Status, Is.EqualTo(BugCreationStatus.ReadyForCreation));
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }
}
