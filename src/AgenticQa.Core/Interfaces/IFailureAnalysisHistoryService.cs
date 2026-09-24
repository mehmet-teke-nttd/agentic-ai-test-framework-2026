using AgenticQa.Core.Models;
using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Interfaces;

public interface IFailureAnalysisHistoryService
{
    Task<string> SaveAnalysisAsync(
        FailureAnalysisResult analysisResult,
        CancellationToken cancellationToken = default);

    Task<string> SaveQaReviewAsync(
        string testId,
        FailureReviewDecision qaDecision,
        string reviewedBy,
        string comment,
        CancellationToken cancellationToken = default);

    Task<FailureAnalysisHistoryEntry?> LoadAsync(
        string testId,
        CancellationToken cancellationToken = default);
}
