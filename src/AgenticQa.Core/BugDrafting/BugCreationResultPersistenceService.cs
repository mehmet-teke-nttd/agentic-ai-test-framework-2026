using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugCreationResultPersistenceService : IBugCreationResultPersistenceService
{
    private readonly string _outputDirectory;

    public BugCreationResultPersistenceService(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
    }

    public async Task<string> SaveAsync(
        ApprovedBugDraft approvedDraft,
        BugCreationResult creationResult,
        DuplicateBugMatch? duplicateMatch = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_outputDirectory);
        var path = Path.Combine(_outputDirectory, $"{approvedDraft.TestId}-bug-result.json");
        BugCreationResultHistory existing = new()
        {
            TestId = approvedDraft.TestId
        };

        if (File.Exists(path))
        {
            var existingJson = await File.ReadAllTextAsync(path, cancellationToken);
            existing = AgenticJsonSerializer.Deserialize<BugCreationResultHistory>(existingJson) ?? existing;
        }

        var updated = existing with
        {
            TestId = approvedDraft.TestId,
            ApprovedDraft = approvedDraft,
            DuplicateCheck = duplicateMatch,
            LatestResult = creationResult,
            Results = [.. existing.Results, creationResult]
        };

        var payload = AgenticJsonSerializer.Serialize(updated);
        await File.WriteAllTextAsync(path, payload, cancellationToken);
        return path;
    }

    public async Task<BugCreationResult?> LoadByCreationKeyAsync(
        string creationKey,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_outputDirectory))
        {
            return null;
        }

        foreach (var file in Directory.GetFiles(_outputDirectory, "*-bug-result.json"))
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            var history = AgenticJsonSerializer.Deserialize<BugCreationResultHistory>(json);
            if (history is null)
            {
                continue;
            }

            var match = history.Results
                .LastOrDefault(result => string.Equals(result.CreationKey, creationKey, StringComparison.Ordinal));
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
