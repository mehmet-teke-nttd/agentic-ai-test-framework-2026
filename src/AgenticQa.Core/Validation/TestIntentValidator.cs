using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Validation;

public sealed class TestIntentValidator : ITestIntentValidator
{
    private readonly ILogger<TestIntentValidator> _logger;
    private readonly IAmbiguityDetector _ambiguityDetector;

    public TestIntentValidator(
        ILogger<TestIntentValidator> logger,
        IAmbiguityDetector? ambiguityDetector = null)
    {
        _logger = logger;
        _ambiguityDetector = ambiguityDetector ?? new NoOpAmbiguityDetector();
    }

    public TestIntentValidationResult Validate(TestIntent testIntent)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ContractSchemaMigration.Normalize(testIntent);

        _logger.LogInformation("Test Intent validation started for TestId={TestId}", testIntent.TestId);

        var findings = new List<(string Field, ClarificationIssueType Type, string Message)>();

        if (!ContractSchemaVersions.IsSupported(testIntent.SchemaVersion))
        {
            findings.Add(("schemaVersion", ClarificationIssueType.Conflicting, $"Unsupported schemaVersion '{testIntent.SchemaVersion}'."));
        }

        if (string.IsNullOrWhiteSpace(testIntent.TestId))
        {
            findings.Add(("testId", ClarificationIssueType.Missing, "Test ID is required."));
        }

        if (string.IsNullOrWhiteSpace(testIntent.Title))
        {
            findings.Add(("title", ClarificationIssueType.Missing, "Title is required."));
        }

        if (!Enum.IsDefined(testIntent.TestType))
        {
            findings.Add(("testType", ClarificationIssueType.Missing, "Test type must be a supported value."));
        }

        if (testIntent.Preconditions is null)
        {
            findings.Add(("preconditions", ClarificationIssueType.Missing, "Preconditions object is required."));
        }

        if (testIntent.TestData is null)
        {
            findings.Add(("testData", ClarificationIssueType.Missing, "Test data object is required."));
        }

        if (testIntent.Actions is null || testIntent.Actions.Count == 0)
        {
            findings.Add(("actions", ClarificationIssueType.Missing, "At least one action is required."));
        }
        else
        {
            var duplicateStepNumbers = testIntent.Actions
                .GroupBy(action => action.Step)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(step => step)
                .ToList();

            foreach (var step in duplicateStepNumbers)
            {
                findings.Add(("actions", ClarificationIssueType.Conflicting, $"Duplicate action step number found: {step}."));
            }

            foreach (var action in testIntent.Actions.OrderBy(action => action.Step))
            {
                if (action.Step <= 0)
                {
                    findings.Add(("actions", ClarificationIssueType.Missing, "Action step number must be a positive integer."));
                }

                if (string.IsNullOrWhiteSpace(action.Action))
                {
                    findings.Add(("actions", ClarificationIssueType.Missing, $"Action text is required for step {action.Step}."));
                }
            }

            var orderedSteps = testIntent.Actions
                .Select(action => action.Step)
                .OrderBy(step => step)
                .ToList();

            if (orderedSteps.Count > 0)
            {
                for (var expected = 1; expected <= orderedSteps.Count; expected++)
                {
                    if (orderedSteps[expected - 1] != expected)
                    {
                        findings.Add(("actions", ClarificationIssueType.Conflicting, "Action step numbers must be sequential (1, 2, 3...)."));
                        break;
                    }
                }
            }
        }

        if (testIntent.ExpectedResults is null || testIntent.ExpectedResults.Count == 0)
        {
            findings.Add(("expectedResults", ClarificationIssueType.Missing, "At least one expected result is required."));
        }

        var ambiguityFindings = _ambiguityDetector.Detect(testIntent);
        foreach (var issue in ambiguityFindings)
        {
            findings.Add((issue.Field, issue.Type, issue.Message));
        }

        var issues = findings
            .Select((finding, index) => new ClarificationIssue
            {
                IssueId = $"ISSUE-{index + 1:000}",
                Field = finding.Field,
                Type = finding.Type,
                Message = finding.Message
            })
            .ToList();

        var status = issues.Count == 0
            ? ValidationStatus.Valid
            : ValidationStatus.NeedsClarification;

        _logger.LogInformation(
            "Validation completed for TestId={TestId}. Status={Status}, IssueCount={IssueCount}",
            testIntent.TestId,
            status,
            issues.Count);

        return new TestIntentValidationResult
        {
            Status = status,
            Issues = issues
        };
    }
}
