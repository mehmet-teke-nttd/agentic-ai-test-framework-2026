using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class TestIntent
{
    public string SchemaVersion { get; set; } = ContractSchemaVersions.Current;
    public string TestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TestType TestType { get; set; }
    public List<string> Preconditions { get; set; } = [];
    public Dictionary<string, object?> TestData { get; set; } = [];
    public List<TestAction> Actions { get; set; } = [];
    public List<string> ExpectedResults { get; set; } = [];
    public List<ApprovedAssumption> ApprovedAssumptions { get; set; } = [];
    public List<ClarificationHistoryEntry> ClarificationHistory { get; set; } = [];
}

public sealed class TestAction
{
    public int Step { get; set; }
    public string Action { get; set; } = string.Empty;
}

public sealed class ClarificationIssue
{
    public string IssueId { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public ClarificationIssueType Type { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class ClarificationResponse
{
    public string IssueId { get; set; } = string.Empty;
    public ClarificationOutcome Outcome { get; set; }
    public string Response { get; set; } = string.Empty;
    public string RespondedBy { get; set; } = string.Empty;
}

public sealed class ApprovedAssumption
{
    public string IssueId { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

public sealed class ClarificationHistoryEntry
{
    public string IssueId { get; set; } = string.Empty;
    public ClarificationOutcome Outcome { get; set; }
    public string Response { get; set; } = string.Empty;
    public string RespondedBy { get; set; } = string.Empty;
}

public sealed class ExecutionContract
{
    public string SchemaVersion { get; set; } = ContractSchemaVersions.Current;
    public string TestId { get; set; } = string.Empty;
    public string ExecutionType { get; set; } = string.Empty;
    public List<ExecutionStep> Steps { get; set; } = [];
}

public sealed class ExecutionStep
{
    public int Step { get; set; }
    public ExecutionKeyword Keyword { get; set; }
    public string? Target { get; set; }
    public string? Value { get; set; }
    public string? Expected { get; set; }
    public int? DependsOn { get; set; }
}

public sealed class ElementRegistry
{
    public List<ElementRegistryEntry> Elements { get; set; } = [];
}

public sealed class ElementRegistryEntry
{
    public string Target { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public LocatorType LocatorType { get; set; }
    public string LocatorValue { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public sealed class StepExecutionResult
{
    public string TestId { get; set; } = string.Empty;
    public int Step { get; set; }
    public ExecutionKeyword Keyword { get; set; }
    public string? Target { get; set; }
    public StepStatus Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public long DurationMs { get; set; }
    public ExecutionError? Error { get; set; }
    public SkipReason? SkipReason { get; set; }
    public int? DependsOn { get; set; }
}

public sealed class ExecutionError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class TestExecutionResult
{
    public string TestId { get; set; } = string.Empty;
    public TestStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public int TotalSteps { get; set; }
    public int PassedSteps { get; set; }
    public int FailedSteps { get; set; }
    public int BlockedSteps { get; set; }
    public int SkippedSteps { get; set; }
    public List<StepExecutionResult> StepResults { get; set; } = [];
    public ExecutionError? Error { get; set; }
}
