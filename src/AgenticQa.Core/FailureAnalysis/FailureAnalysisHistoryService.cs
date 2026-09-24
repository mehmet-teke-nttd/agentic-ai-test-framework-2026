using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class FailureAnalysisHistoryService : IFailureAnalysisHistoryService
{
    private readonly string _outputDirectory;
    private readonly ILogger<FailureAnalysisHistoryService> _logger;

    public FailureAnalysisHistoryService(
        string outputDirectory,
        ILogger<FailureAnalysisHistoryService> logger)
    {
        _outputDirectory = outputDirectory;
        _logger = logger;
    }

    public async Task<string> SaveAnalysisAsync(
        FailureAnalysisResult analysisResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysisResult);

        var existing = await LoadAsync(analysisResult.TestId, cancellationToken);
        var entry = new FailureAnalysisHistoryEntry
        {
            TestId = analysisResult.TestId,
            AnalysisResult = analysisResult,
            AnalyzedAt = DateTimeOffset.UtcNow,
            QaReview = existing?.QaReview
        };

        return await WriteAsync(entry, cancellationToken);
    }

    public async Task<string> SaveQaReviewAsync(
        string testId,
        FailureReviewDecision qaDecision,
        string reviewedBy,
        string comment,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewedBy);
        ArgumentNullException.ThrowIfNull(comment);

        var existing = await LoadAsync(testId, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException($"No failure analysis found for TestId '{testId}'.");
        }

        var review = new FailureReviewRecord
        {
            TestId = testId,
            AgentClassification = existing.AnalysisResult.Classification,
            QaDecision = qaDecision,
            ReviewedBy = reviewedBy,
            Comment = comment,
            ReviewedAt = DateTimeOffset.UtcNow
        };

        var updated = new FailureAnalysisHistoryEntry
        {
            TestId = existing.TestId,
            AnalysisResult = existing.AnalysisResult,
            AnalyzedAt = existing.AnalyzedAt,
            QaReview = review
        };

        _logger.LogInformation("QA review recorded for TestId={TestId}", testId);
        return await WriteAsync(updated, cancellationToken);
    }

    public async Task<FailureAnalysisHistoryEntry?> LoadAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(testId);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return AgenticJsonSerializer.Deserialize<FailureAnalysisHistoryEntry>(json);
    }

    private async Task<string> WriteAsync(FailureAnalysisHistoryEntry entry, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_outputDirectory);
        var path = GetFilePath(entry.TestId);
        var payload = AgenticJsonSerializer.Serialize(entry);
        await File.WriteAllTextAsync(path, payload, cancellationToken);
        return path;
    }

    private string GetFilePath(string testId) =>
        Path.Combine(_outputDirectory, $"{testId}-analysis.json");
}
