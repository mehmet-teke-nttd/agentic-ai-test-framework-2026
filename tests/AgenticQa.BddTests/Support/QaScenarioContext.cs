using AgenticQa.Core.Models;

namespace AgenticQa.BddTests.Support;

public sealed class QaScenarioContext
{
    public DateTimeOffset ScenarioStartedAt { get; set; } = DateTimeOffset.UtcNow;
    public TestIntent? TestIntent { get; set; }
    public TestIntentValidationResult? ValidationResult { get; set; }
    public ExecutionContract? ExecutionContract { get; set; }
    public ExecutionContractValidationResult? ExecutionContractValidationResult { get; set; }
    public TestExecutionResult? TestExecutionResult { get; set; }
    public FailureAnalysisResult? FailureAnalysisResult { get; set; }
    public string? ResultFilePath { get; set; }
    public string? FailureAnalysisPath { get; set; }
    public string? ScreenshotPath { get; set; }
    public string? AgentRunPath { get; set; }
}
