using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class TestDataResolutionResult
{
    public bool IsSuccess { get; init; }
    public string? ResolvedValue { get; init; }
    public RuntimeExecutionErrorCode? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}
