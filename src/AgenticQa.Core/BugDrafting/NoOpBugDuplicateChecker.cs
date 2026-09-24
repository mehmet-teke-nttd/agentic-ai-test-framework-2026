using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.BugDrafting;

public sealed class NoOpBugDuplicateChecker : IBugDuplicateChecker
{
    public Task<DuplicateBugMatch> FindDuplicateAsync(
        ApprovedBugDraft draft,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new DuplicateBugMatch { IsDuplicate = false });
}
