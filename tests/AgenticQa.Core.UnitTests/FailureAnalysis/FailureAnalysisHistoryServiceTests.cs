using AgenticQa.Core.Enums;
using AgenticQa.Core.FailureAnalysis;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.FailureAnalysis;

public class FailureAnalysisHistoryServiceTests
{
    [Test]
    public async Task QaReview_IsStoredSeparately_AndOriginalAnalysisRemainsUnchanged()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"agentic-failure-analysis-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDir);

        try
        {
            var service = new FailureAnalysisHistoryService(outputDir, NullLogger<FailureAnalysisHistoryService>.Instance);
            var analysis = new AgenticQa.Core.Models.FailureAnalysisResult
            {
                TestId = "TC-LOGIN-001",
                Classification = FailureClassification.ProductDefect,
                Confidence = 0.81,
                Summary = "Dashboard was not displayed.",
                Evidence = ["Step 4 passed", "Step 5 failed"],
                RecommendedAction = FailureRecommendedAction.QaReviewRequired
            };

            await service.SaveAnalysisAsync(analysis);
            await service.SaveQaReviewAsync(
                "TC-LOGIN-001",
                FailureReviewDecision.Confirmed,
                "QA_ENGINEER",
                "Reproduced manually.");

            var loaded = await service.LoadAsync("TC-LOGIN-001");
            Assert.That(loaded, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(loaded!.AnalysisResult.Classification, Is.EqualTo(FailureClassification.ProductDefect));
                Assert.That(loaded.QaReview, Is.Not.Null);
                Assert.That(loaded.QaReview!.QaDecision, Is.EqualTo(FailureReviewDecision.Confirmed));
                Assert.That(loaded.QaReview.AgentClassification, Is.EqualTo(FailureClassification.ProductDefect));
            });
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
