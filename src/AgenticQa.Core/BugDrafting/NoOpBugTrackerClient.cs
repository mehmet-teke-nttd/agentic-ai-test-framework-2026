using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Enums;

namespace AgenticQa.Core.BugDrafting;

public sealed class NoOpBugTrackerClient : IBugTrackerClient
{
    public Task<BugCreationResult> CreateBugAsync(ApprovedBugDraft draft, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BugCreationResult
        {
            TestId = draft.TestId,
            CreationKey = draft.CreationKey,
            Status = BugCreationStatus.Blocked,
            Message = "External bug tracker integration is disabled in this phase."
        });
    }
}
