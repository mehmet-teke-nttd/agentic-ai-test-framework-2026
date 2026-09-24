using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugDraftPersistenceServiceTests
{
    [Test]
    public async Task ReviewUpdatesStatusAndPreservesOriginalDraft()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"agentic-bug-drafts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);

        try
        {
            var service = new BugDraftPersistenceService(dir, NullLogger<BugDraftPersistenceService>.Instance);
            var recommendation = new BugRecommendationResult
            {
                TestId = "TC-1",
                Status = BugRecommendationStatus.BugDraftRecommended,
                Reason = "confirmed product defect"
            };
            var originalDraft = new BugDraft
            {
                TestId = "TC-1",
                Title = "Original title",
                Summary = "Original summary",
                ExpectedResult = "Expected",
                ActualResult = "Actual",
                Environment = "UI"
            };

            await service.SaveDraftAsync(recommendation, originalDraft);
            await service.SaveReviewAsync(
                "TC-1",
                BugDraftReviewDecision.EditedAndApproved,
                "QA_ENGINEER",
                "Edited wording.",
                new BugDraft
                {
                    TestId = "TC-1",
                    Title = "Edited title",
                    Summary = "Edited summary",
                    ExpectedResult = "Expected",
                    ActualResult = "Actual",
                    Environment = "UI"
                });

            var loaded = await service.LoadAsync("TC-1");
            Assert.That(loaded, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(loaded!.OriginalDraft.Title, Is.EqualTo("Original title"));
                Assert.That(loaded.Status, Is.EqualTo(BugDraftStatus.Approved));
                Assert.That(loaded.QaReviewedDraft, Is.Not.Null);
                Assert.That(loaded.QaReviewedDraft!.Title, Is.EqualTo("Edited title"));
            });
        }
        finally
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
