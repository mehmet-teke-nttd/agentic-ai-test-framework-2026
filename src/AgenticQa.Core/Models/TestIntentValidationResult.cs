using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class TestIntentValidationResult
{
    public ValidationStatus Status { get; init; }
    public IReadOnlyList<ClarificationIssue> Issues { get; init; } = [];
}
