using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class FailureClassificationService : IFailureClassificationService
{
    private readonly ILogger<FailureClassificationService> _logger;
    private readonly IFailureAnalysisAgent _failureAnalysisAgent;

    public FailureClassificationService(
        ILogger<FailureClassificationService> logger,
        IFailureAnalysisAgent failureAnalysisAgent)
    {
        _logger = logger;
        _failureAnalysisAgent = failureAnalysisAgent;
    }

    public async Task<FailureAnalysisResult?> ClassifyIfNeededAsync(
        TestIntent testIntent,
        ExecutionContract executionContract,
        TestExecutionResult testExecutionResult,
        string? currentPage = null,
        string? screenshotPath = null,
        IReadOnlyList<string>? relevantLogs = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(executionContract);
        ArgumentNullException.ThrowIfNull(testExecutionResult);

        if (testExecutionResult.Status == TestStatus.Passed)
        {
            return null;
        }

        _logger.LogInformation("Deterministic classification attempted for TestId={TestId}", testExecutionResult.TestId);

        var failedStep = testExecutionResult.StepResults
            .FirstOrDefault(step => step.Status is StepStatus.Failed or StepStatus.Blocked);
        var primaryErrorCode = failedStep?.Error?.ErrorCode ?? testExecutionResult.Error?.ErrorCode;
        var primaryErrorMessage = failedStep?.Error?.Message ?? testExecutionResult.Error?.Message ?? string.Empty;

        if (TryDeterministicClassification(
            testExecutionResult.TestId,
            primaryErrorCode,
            primaryErrorMessage,
            failedStep,
            out var deterministic))
        {
            _logger.LogInformation("Deterministic classification succeeded for TestId={TestId}", testExecutionResult.TestId);
            return deterministic;
        }

        var request = BuildRequest(
            testIntent,
            testExecutionResult,
            currentPage,
            screenshotPath,
            relevantLogs ?? []);

        return await _failureAnalysisAgent.AnalyzeAsync(request, cancellationToken);
    }

    private static FailureAnalysisRequest BuildRequest(
        TestIntent testIntent,
        TestExecutionResult testExecutionResult,
        string? currentPage,
        string? screenshotPath,
        IReadOnlyList<string> logs)
    {
        var failedStep = testExecutionResult.StepResults
            .FirstOrDefault(step => step.Status is StepStatus.Failed or StepStatus.Blocked);

        return new FailureAnalysisRequest
        {
            TestId = testExecutionResult.TestId,
            Title = testIntent.Title,
            TestType = testIntent.TestType,
            ExpectedResults = testIntent.ExpectedResults,
            CurrentPage = currentPage,
            ScreenshotPath = screenshotPath,
            RelevantLogs = logs,
            FailedStep = failedStep is null ? null : Snapshot(failedStep),
            PreviousStepResults = testExecutionResult.StepResults
                .Where(step => failedStep is null || step.Step < failedStep.Step)
                .Select(Snapshot)
                .ToList()
        };
    }

    private static FailureAnalysisStepSnapshot Snapshot(StepExecutionResult step) =>
        new()
        {
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            StepStatus = step.Status,
            ErrorCode = step.Error?.ErrorCode,
            ErrorMessage = step.Error?.Message
        };

    private static bool TryDeterministicClassification(
        string testId,
        string? errorCode,
        string errorMessage,
        StepExecutionResult? failedStep,
        out FailureAnalysisResult result)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            result = default!;
            return false;
        }

        var normalizedCode = errorCode.Trim().ToUpperInvariant();
        if (normalizedCode is "LOCATOR_NOT_FOUND"
            or "LOCATOR_NOT_UNIQUE"
            or "TARGET_NOT_REGISTERED"
            or "PAGE_MISMATCH"
            or "INVALID_LOCATOR_CONFIG")
        {
            result = Build(
                testId,
                FailureClassification.AutomationIssue,
                0.92,
                "Execution was blocked by locator or element configuration issues.",
                FailureRecommendedAction.InvestigateAutomation,
                failedStep,
                errorCode);
            return true;
        }

        if (normalizedCode == "TEST_DATA_REFERENCE_NOT_FOUND")
        {
            result = Build(
                testId,
                FailureClassification.TestDataIssue,
                0.93,
                "Execution could not resolve required test data.",
                FailureRecommendedAction.CheckTestData,
                failedStep,
                errorCode);
            return true;
        }

        if (normalizedCode is "NAVIGATION_FAILED"
            or "PAGE_NOT_REGISTERED"
            or "EXECUTION_CONFIGURATION_ERROR"
            or "EXECUTION_CANCELLED")
        {
            result = Build(
                testId,
                FailureClassification.EnvironmentIssue,
                0.88,
                "Execution was impacted by environment or runtime infrastructure conditions.",
                FailureRecommendedAction.CheckEnvironment,
                failedStep,
                errorCode);
            return true;
        }

        if (normalizedCode == "TEST_INTENT_INVALID"
            || errorMessage.Contains("requirement", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("ambiguous", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("conflict", StringComparison.OrdinalIgnoreCase))
        {
            result = Build(
                testId,
                FailureClassification.RequirementIssue,
                0.84,
                "The available evidence indicates unclear or conflicting expected behavior.",
                FailureRecommendedAction.ReviewRequirements,
                failedStep,
                errorCode);
            return true;
        }

        result = default!;
        return false;
    }

    private static FailureAnalysisResult Build(
        string testId,
        FailureClassification classification,
        double confidence,
        string summary,
        FailureRecommendedAction action,
        StepExecutionResult? failedStep,
        string errorCode)
    {
        var evidence = new List<string>
        {
            $"Primary error code: {errorCode}."
        };

        if (failedStep is not null)
        {
            evidence.Add($"Failure observed at step {failedStep.Step} ({failedStep.Keyword}).");
        }

        var escalation = FailureEscalationPolicy.Evaluate(classification, confidence, action);

        return new FailureAnalysisResult
        {
            TestId = testId,
            Classification = classification,
            Confidence = confidence,
            Summary = summary,
            Evidence = evidence,
            RecommendedAction = action,
            EscalationRequired = escalation.EscalationRequired,
            EscalationLevel = escalation.EscalationLevel,
            EscalationReason = escalation.EscalationReason
        };
    }
}
