using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class LocatorValidationResult
{
    public LocatorValidationStatus Status { get; init; }
    public string Target { get; init; } = string.Empty;
    public string ExpectedPage { get; init; } = string.Empty;
    public int? MatchCount { get; init; }
    public ExecutionError? Error { get; init; }
}
