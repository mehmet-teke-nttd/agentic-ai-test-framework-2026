using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugCreationService : IBugCreationService
{
    private readonly ILogger<BugCreationService> _logger;
    private readonly IBugCreationGuard _guard;
    private readonly IBugDuplicateChecker _duplicateChecker;
    private readonly IBugTrackerClient _bugTrackerClient;
    private readonly IBugCreationResultPersistenceService _resultPersistence;

    public BugCreationService(
        ILogger<BugCreationService> logger,
        IBugCreationGuard guard,
        IBugDuplicateChecker duplicateChecker,
        IBugTrackerClient bugTrackerClient,
        IBugCreationResultPersistenceService resultPersistence)
    {
        _logger = logger;
        _guard = guard;
        _duplicateChecker = duplicateChecker;
        _bugTrackerClient = bugTrackerClient;
        _resultPersistence = resultPersistence;
    }

    public async Task<BugCreationResult> CreateIfApprovedAsync(
        BugRecommendationResult recommendation,
        BugDraftHistoryEntry draftHistory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bug creation requested for TestId={TestId}", draftHistory.TestId);

        var guardResult = _guard.Validate(recommendation, draftHistory);
        if (guardResult.Status == BugCreationStatus.Blocked)
        {
            return guardResult;
        }

        var draftToCreate = draftHistory.QaReviewedDraft
            ?? draftHistory.AiPolishedDraft
            ?? draftHistory.OriginalDraft;
        var review = draftHistory.Review!;
        var creationKey = $"{draftHistory.TestId}:{review.ReviewedAt:O}";

        var existing = await _resultPersistence.LoadByCreationKeyAsync(creationKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var approved = new ApprovedBugDraft
        {
            TestId = draftHistory.TestId,
            CreationKey = creationKey,
            Draft = draftToCreate,
            Review = review,
            FailureClassification = draftToCreate.FailureClassification
        };

        _logger.LogInformation("Duplicate check started for TestId={TestId}", draftHistory.TestId);
        var duplicate = await _duplicateChecker.FindDuplicateAsync(approved, cancellationToken);
        if (duplicate.IsDuplicate)
        {
            _logger.LogInformation("Duplicate found for TestId={TestId}", draftHistory.TestId);
            var duplicateResult = new BugCreationResult
            {
                TestId = draftHistory.TestId,
                CreationKey = creationKey,
                Status = BugCreationStatus.Duplicate,
                ExistingBugId = duplicate.ExistingBugId,
                ExistingBugUrl = duplicate.ExistingBugUrl,
                Message = duplicate.Reason ?? "Duplicate bug found.",
                Error = new ExecutionError
                {
                    ErrorCode = "DUPLICATE_BUG_FOUND",
                    Message = duplicate.Reason ?? "Duplicate bug found."
                }
            };

            await _resultPersistence.SaveAsync(approved, duplicateResult, duplicate, cancellationToken);
            return duplicateResult;
        }

        var creationResult = await _bugTrackerClient.CreateBugAsync(approved, cancellationToken);
        await _resultPersistence.SaveAsync(approved, creationResult, duplicate, cancellationToken);
        return creationResult;
    }
}
