using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugRecommendationService
{
    BugRecommendationResult Recommend(
        TestIntent testIntent,
        TestExecutionResult testExecutionResult,
        FailureAnalysisResult failureAnalysisResult,
        FailureReviewDecision failureReviewDecision);
}
