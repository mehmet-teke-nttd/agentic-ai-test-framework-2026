using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class ExecutionContractMappingIssue
{
    public int Step { get; init; }
    public string Action { get; init; } = string.Empty;
    public ExecutionContractValidationErrorCode ErrorCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ExecutionContractMappingResult
{
    public MappingStatus Status { get; init; }
    public string TestId { get; init; } = string.Empty;
    public ExecutionContract? ExecutionContract { get; init; }
    public IReadOnlyList<ExecutionContractMappingIssue> Issues { get; init; } = [];
}
