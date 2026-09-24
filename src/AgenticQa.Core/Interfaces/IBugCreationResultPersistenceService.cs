using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugCreationResultPersistenceService
{
    Task<string> SaveAsync(
        ApprovedBugDraft approvedDraft,
        BugCreationResult creationResult,
        DuplicateBugMatch? duplicateMatch = null,
        CancellationToken cancellationToken = default);

    Task<BugCreationResult?> LoadByCreationKeyAsync(
        string creationKey,
        CancellationToken cancellationToken = default);
}
