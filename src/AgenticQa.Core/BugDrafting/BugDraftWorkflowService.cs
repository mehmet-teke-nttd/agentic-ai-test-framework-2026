using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugDraftWorkflowService
{
    private readonly ILogger<BugDraftWorkflowService> _logger;
    private readonly IBugTrackerClient _bugTrackerClient;

    public BugDraftWorkflowService(
        ILogger<BugDraftWorkflowService> logger,
        IBugTrackerClient bugTrackerClient)
    {
        _logger = logger;
        _bugTrackerClient = bugTrackerClient;
    }

    public async Task<BugCreationResult> CreateBugIfApprovedAsync(
        BugDraftHistoryEntry draftHistory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draftHistory);

        if (draftHistory.Status != BugDraftStatus.Approved
            || draftHistory.Review is null
            || draftHistory.Review.Decision is not (BugDraftReviewDecision.Approved or BugDraftReviewDecision.EditedAndApproved))
        {
            _logger.LogInformation("Unapproved bug draft blocked from external creation for TestId={TestId}", draftHistory.TestId);
            return new BugCreationResult
            {
                TestId = draftHistory.TestId,
                Status = BugCreationStatus.Blocked,
                Message = "Bug draft is not approved by QA.",
                Error = new ExecutionError
                {
                    ErrorCode = "BUG_NOT_APPROVED",
                    Message = "Bug draft is not approved by QA."
                }
            };
        }

        var draftToCreate = draftHistory.QaReviewedDraft
            ?? draftHistory.AiPolishedDraft
            ?? draftHistory.OriginalDraft;

        var approved = new ApprovedBugDraft
        {
            TestId = draftHistory.TestId,
            CreationKey = $"{draftHistory.TestId}:{draftHistory.Review.ReviewedAt:O}",
            Draft = draftToCreate,
            Review = draftHistory.Review,
            FailureClassification = draftToCreate.FailureClassification
        };

        return await _bugTrackerClient.CreateBugAsync(approved, cancellationToken);
    }
}
