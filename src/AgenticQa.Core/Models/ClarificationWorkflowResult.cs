using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class ClarificationWorkflowResult
{
    public bool IsAccepted { get; init; }
    public bool IsBlocked { get; init; }
    public string? RejectionReason { get; init; }
    public TestIntent UpdatedIntent { get; init; } = new();
    public TestIntentValidationResult ValidationResult { get; init; } = new()
    {
        Status = ValidationStatus.NeedsClarification
    };
}
