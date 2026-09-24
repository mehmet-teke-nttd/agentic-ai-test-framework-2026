using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class ElementRegistryValidationError
{
    public ElementRegistryValidationErrorCode ErrorCode { get; init; }
    public string? Target { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ElementRegistryValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<ElementRegistryValidationError> Errors { get; init; } = [];
}
