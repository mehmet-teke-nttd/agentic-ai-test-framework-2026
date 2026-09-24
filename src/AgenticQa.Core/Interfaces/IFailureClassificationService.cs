using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IFailureClassificationService
{
    Task<FailureAnalysisResult?> ClassifyIfNeededAsync(
        TestIntent testIntent,
        ExecutionContract executionContract,
        TestExecutionResult testExecutionResult,
        string? currentPage = null,
        string? screenshotPath = null,
        IReadOnlyList<string>? relevantLogs = null,
        CancellationToken cancellationToken = default);
}
