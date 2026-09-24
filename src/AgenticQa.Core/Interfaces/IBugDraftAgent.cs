using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugDraftAgent
{
    Task<BugDraftPolishResult?> PolishAsync(
        BugDraftRequest request,
        CancellationToken cancellationToken = default);
}
