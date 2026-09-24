using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugDuplicateChecker
{
    Task<DuplicateBugMatch> FindDuplicateAsync(
        ApprovedBugDraft draft,
        CancellationToken cancellationToken = default);
}
