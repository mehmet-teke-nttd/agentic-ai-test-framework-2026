using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugRecommendationService : IBugRecommendationService
{
    private readonly ILogger<BugRecommendationService> _logger;

    public BugRecommendationService(ILogger<BugRecommendationService> logger)
    {
        _logger = logger;
    }

    public BugRecommendationResult Recommend(
        TestIntent testIntent,
        TestExecutionResult testExecutionResult,
        FailureAnalysisResult failureAnalysisResult,
        FailureReviewDecision failureReviewDecision)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(testExecutionResult);
        ArgumentNullException.ThrowIfNull(failureAnalysisResult);

        _logger.LogInformation("Bug recommendation started for TestId={TestId}", testExecutionResult.TestId);

        var status = failureAnalysisResult.Classification switch
        {
            FailureClassification.ProductDefect when failureReviewDecision == FailureReviewDecision.Confirmed
                => BugRecommendationStatus.BugDraftRecommended,
            FailureClassification.ProductDefect when failureReviewDecision == FailureReviewDecision.NeedsMoreInvestigation
                => BugRecommendationStatus.NoBugYet,
            _ => BugRecommendationStatus.NoBug
        };

        var reason = status switch
        {
            BugRecommendationStatus.BugDraftRecommended => "Failure classified as PRODUCT_DEFECT and confirmed by QA.",
            BugRecommendationStatus.NoBugYet => "Failure classified as PRODUCT_DEFECT but QA requested further investigation.",
            _ => $"No bug draft is recommended for classification {failureAnalysisResult.Classification} with QA decision {failureReviewDecision}."
        };

        _logger.LogInformation(
            status == BugRecommendationStatus.BugDraftRecommended
                ? "Bug draft recommended for TestId={TestId}"
                : "Bug draft not recommended for TestId={TestId}",
            testExecutionResult.TestId);

        return new BugRecommendationResult
        {
            TestId = testExecutionResult.TestId,
            Status = status,
            Reason = reason
        };
    }
}
