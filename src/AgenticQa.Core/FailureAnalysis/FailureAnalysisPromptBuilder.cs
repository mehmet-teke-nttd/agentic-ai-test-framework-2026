using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class FailureAnalysisPromptBuilder : IFailureAnalysisPromptBuilder
{
    public FailureAnalysisPrompt Build(FailureAnalysisRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var failedStepSection = request.FailedStep is null
            ? "failedStep: null"
            : $"""
              failedStep:
                step: {request.FailedStep.Step}
                keyword: {request.FailedStep.Keyword}
                target: {request.FailedStep.Target ?? "null"}
                status: {request.FailedStep.StepStatus}
                errorCode: {request.FailedStep.ErrorCode ?? "null"}
                errorMessage: {request.FailedStep.ErrorMessage ?? "null"}
              """;

        var previousSteps = request.PreviousStepResults.Count == 0
            ? "[]"
            : string.Join(
                Environment.NewLine,
                request.PreviousStepResults.Select(
                    step => $"- step={step.Step}, keyword={step.Keyword}, status={step.StepStatus}, errorCode={step.ErrorCode ?? "null"}"));

        var logs = request.RelevantLogs.Count == 0
            ? "[]"
            : string.Join(Environment.NewLine, request.RelevantLogs.Select(line => $"- {line}"));

        var expected = request.ExpectedResults.Count == 0
            ? "[]"
            : string.Join(Environment.NewLine, request.ExpectedResults.Select(line => $"- {line}"));

        var systemPrompt =
            """
            You are a QA failure analysis assistant.
            Analyze only the provided evidence.
            Do not assume missing facts.
            Do not claim a bug is confirmed.
            Distinguish among PRODUCT_DEFECT, AUTOMATION_ISSUE, TEST_DATA_ISSUE, ENVIRONMENT_ISSUE, REQUIREMENT_ISSUE, UNKNOWN.
            Prefer UNKNOWN when evidence is insufficient.
            Return STRICT JSON only with keys:
            classification, confidence, summary, evidence, recommendedAction, escalationRequired, escalationLevel, escalationReason.
            Confidence must be between 0 and 1.
            recommendedAction must be one of:
            QA_REVIEW_REQUIRED, INVESTIGATE_AUTOMATION, CHECK_TEST_DATA, CHECK_ENVIRONMENT, REVIEW_REQUIREMENTS, NO_ACTION.
            escalationLevel must be one of:
            NONE, LOW, MEDIUM, HIGH, CRITICAL.
            """;

        var userPrompt =
            $"""
             testId: {request.TestId}
             title: {request.Title}
             testType: {request.TestType}
             expectedResults:
             {expected}
             {failedStepSection}
             previousStepResults:
             {previousSteps}
             currentPage: {request.CurrentPage ?? "null"}
             screenshotPath: {request.ScreenshotPath ?? "null"}
             relevantLogs:
             {logs}
             """;

        return new FailureAnalysisPrompt
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };
    }
}
