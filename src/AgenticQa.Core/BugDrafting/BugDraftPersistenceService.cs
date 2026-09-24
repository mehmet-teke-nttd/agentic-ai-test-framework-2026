using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugDraftPersistenceService : IBugDraftPersistenceService
{
    private readonly string _outputDirectory;
    private readonly ILogger<BugDraftPersistenceService> _logger;

    public BugDraftPersistenceService(
        string outputDirectory,
        ILogger<BugDraftPersistenceService> logger)
    {
        _outputDirectory = outputDirectory;
        _logger = logger;
    }

    public async Task<string> SaveDraftAsync(
        BugRecommendationResult recommendation,
        BugDraft originalDraft,
        BugDraft? aiPolishedDraft = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentNullException.ThrowIfNull(originalDraft);

        var existing = await LoadAsync(originalDraft.TestId, cancellationToken);
        var entry = new BugDraftHistoryEntry
        {
            TestId = originalDraft.TestId,
            Recommendation = recommendation,
            OriginalDraft = existing?.OriginalDraft ?? originalDraft,
            AiPolishedDraft = aiPolishedDraft ?? existing?.AiPolishedDraft,
            QaReviewedDraft = existing?.QaReviewedDraft,
            Status = existing?.Status ?? BugDraftStatus.Draft,
            Review = existing?.Review,
            CreatedAt = existing?.CreatedAt ?? DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Bug draft created for TestId={TestId}", originalDraft.TestId);
        return await WriteAsync(entry, cancellationToken);
    }

    public async Task<string> SaveReviewAsync(
        string testId,
        BugDraftReviewDecision decision,
        string reviewedBy,
        string comment,
        BugDraft? reviewedDraft = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewedBy);

        var existing = await LoadAsync(testId, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException($"No bug draft exists for TestId '{testId}'.");
        }

        var status = decision switch
        {
            BugDraftReviewDecision.Approved => BugDraftStatus.Approved,
            BugDraftReviewDecision.EditedAndApproved => BugDraftStatus.Approved,
            _ => BugDraftStatus.Rejected
        };

        var updated = new BugDraftHistoryEntry
        {
            TestId = existing.TestId,
            Recommendation = existing.Recommendation,
            OriginalDraft = existing.OriginalDraft,
            AiPolishedDraft = existing.AiPolishedDraft,
            QaReviewedDraft = reviewedDraft ?? existing.QaReviewedDraft,
            Status = status,
            Review = new BugDraftReview
            {
                TestId = testId,
                Decision = decision,
                ReviewedBy = reviewedBy,
                Comment = comment,
                ReviewedAt = DateTimeOffset.UtcNow
            },
            CreatedAt = existing.CreatedAt
        };

        _logger.LogInformation(
            status == BugDraftStatus.Approved ? "Bug draft approved for TestId={TestId}" : "Bug draft rejected for TestId={TestId}",
            testId);

        return await WriteAsync(updated, cancellationToken);
    }

    public async Task<BugDraftHistoryEntry?> LoadAsync(string testId, CancellationToken cancellationToken = default)
    {
        var path = GetPath(testId);
        if (!File.Exists(path))
        {
            return null;
        }

        var payload = await File.ReadAllTextAsync(path, cancellationToken);
        return AgenticJsonSerializer.Deserialize<BugDraftHistoryEntry>(payload);
    }

    private async Task<string> WriteAsync(BugDraftHistoryEntry entry, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_outputDirectory);
        var path = GetPath(entry.TestId);
        var payload = AgenticJsonSerializer.Serialize(entry);
        await File.WriteAllTextAsync(path, payload, cancellationToken);
        return path;
    }

    private string GetPath(string testId) =>
        Path.Combine(_outputDirectory, $"{testId}-bug-draft.json");
}
