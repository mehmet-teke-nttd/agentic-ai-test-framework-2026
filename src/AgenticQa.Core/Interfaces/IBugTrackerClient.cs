using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugTrackerClient
{
    Task<BugCreationResult> CreateBugAsync(
        ApprovedBugDraft draft,
        CancellationToken cancellationToken = default);
}
