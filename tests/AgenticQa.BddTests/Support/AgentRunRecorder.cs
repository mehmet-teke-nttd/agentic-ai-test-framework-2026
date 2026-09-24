using AgenticQa.Core.Serialization;

namespace AgenticQa.BddTests.Support;

public static class AgentRunRecorder
{
    public static async Task<string> SaveAsync(
        string outputDirectory,
        AgentRunRecord record,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.RunId);

        Directory.CreateDirectory(outputDirectory);
        var filePath = Path.Combine(outputDirectory, $"{record.RunId}.json");
        var payload = AgenticJsonSerializer.Serialize(record);
        await File.WriteAllTextAsync(filePath, payload, cancellationToken);
        return filePath;
    }

    public static string CreateRunId(string scenarioName)
    {
        var normalized = new string(
            (scenarioName ?? string.Empty)
            .Trim()
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-')
            .ToArray())
            .Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "scenario";
        }

        var runId = $"{DateTimeOffset.UtcNow:yyyyMMddTHHmmssfffZ}-{normalized}-{Guid.NewGuid():N}";
        return runId.Length <= 96 ? runId : runId[..96];
    }
}
