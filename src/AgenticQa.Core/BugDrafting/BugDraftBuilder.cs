using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugDraftBuilder : IBugDraftBuilder
{
    private readonly ILogger<BugDraftBuilder> _logger;

    public BugDraftBuilder(ILogger<BugDraftBuilder> logger)
    {
        _logger = logger;
    }

    public BugDraft Build(
        TestIntent testIntent,
        ExecutionContract executionContract,
        TestExecutionResult testExecutionResult,
        FailureAnalysisResult failureAnalysisResult,
        IReadOnlyList<string>? artifactReferences = null)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ArgumentNullException.ThrowIfNull(executionContract);
        ArgumentNullException.ThrowIfNull(testExecutionResult);
        ArgumentNullException.ThrowIfNull(failureAnalysisResult);

        var failedStep = testExecutionResult.StepResults
            .FirstOrDefault(step => step.Status is StepStatus.Failed or StepStatus.Blocked);
        var failedStepNumber = failedStep?.Step;
        var failedStepError = failedStep?.Error?.Message ?? testExecutionResult.Error?.Message ?? "Failure details unavailable.";

        var title = string.IsNullOrWhiteSpace(testIntent.Title)
            ? $"Test failure observed for {testIntent.TestId}"
            : testIntent.Title;

        var expected = testIntent.ExpectedResults.FirstOrDefault()
            ?? "Expected behavior should match acceptance criteria.";

        var actual = failedStep is null
            ? failedStepError
            : $"Step {failedStep.Step} ({failedStep.Keyword}) failed: {failedStepError}";

        var evidence = new List<string>();
        evidence.AddRange(testExecutionResult.StepResults.Select(step =>
            $"Step {step.Step} {step.Keyword}: {step.Status}" +
            (step.Error is null ? string.Empty : $" ({step.Error.ErrorCode})")));
        evidence.AddRange(failureAnalysisResult.Evidence);

        var actions = testIntent.Actions
            .OrderBy(action => action.Step)
            .Select(action => action.Action)
            .ToList();

        var draft = new BugDraft
        {
            Title = title,
            Summary = failureAnalysisResult.Summary,
            StepsToReproduce = actions,
            ExpectedResult = expected,
            ActualResult = actual,
            Severity = BugSeverity.Medium,
            Priority = BugPriority.Unassigned,
            Environment = executionContract.ExecutionType,
            TestId = testIntent.TestId,
            FailedStep = failedStepNumber,
            Evidence = evidence,
            Attachments = artifactReferences?.ToList() ?? [],
            RelatedAcceptanceCriteria = testIntent.ExpectedResults.ToList(),
            FailureClassification = failureAnalysisResult.Classification,
            FailureConfidence = failureAnalysisResult.Confidence
        };

        _logger.LogInformation("Bug draft created for TestId={TestId}", testIntent.TestId);
        return draft;
    }
}
