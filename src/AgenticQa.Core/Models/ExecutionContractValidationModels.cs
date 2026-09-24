using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class ExecutionContractValidationError
{
    public ExecutionContractValidationErrorCode ErrorCode { get; init; }
    public int? Step { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ExecutionStepValidationResult
{
    public bool IsValid { get; init; }
    public int Step { get; init; }
    public IReadOnlyList<ExecutionContractValidationError> Errors { get; init; } = [];
}

public sealed class ExecutionContractValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<ExecutionContractValidationError> Errors { get; init; } = [];
}
