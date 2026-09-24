using AgenticQa.Core.Enums;

namespace AgenticQa.BddTests.Support;

public sealed class AgentRunRecord
{
    public string RunId { get; init; } = string.Empty;
    public string ScenarioName { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string TestId { get; init; } = string.Empty;
    public TestStatus? TestStatus { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public double? Confidence { get; init; }
    public bool? EscalationRequired { get; init; }
    public FailureEscalationLevel? EscalationLevel { get; init; }
    public string? EscalationReason { get; init; }
    public string? RecommendedAction { get; init; }
    public string? ResultFilePath { get; init; }
    public string? FailureAnalysisPath { get; init; }
    public string? ScreenshotPath { get; init; }
}
