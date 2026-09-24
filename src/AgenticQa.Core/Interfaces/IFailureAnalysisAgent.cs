using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IFailureAnalysisAgent
{
    Task<FailureAnalysisResult> AnalyzeAsync(
        FailureAnalysisRequest request,
        CancellationToken cancellationToken = default);
}
