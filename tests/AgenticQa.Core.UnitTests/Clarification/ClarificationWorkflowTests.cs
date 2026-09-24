using AgenticQa.Core.Clarification;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using AgenticQa.Core.Validation;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Clarification;

public class ClarificationWorkflowTests
{
    private TestIntentValidator _validator = null!;
    private ClarificationService _clarificationService = null!;
    private TestIntentRevalidationService _revalidationService = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new TestIntentValidator(NullLogger<TestIntentValidator>.Instance);
        _clarificationService = new ClarificationService(
            NullLogger<ClarificationService>.Instance,
            _validator);
        _revalidationService = new TestIntentRevalidationService(
            NullLogger<TestIntentRevalidationService>.Instance,
            _clarificationService);
    }

    [Test]
    public void ValidClarification_UpdatesFieldAndRevalidates()
    {
        var intent = CreateValidIntent();
        intent.Title = string.Empty;
        var validation = _validator.Validate(intent);
        var titleIssue = validation.Issues.Single(i => i.Field == "title");

        var response = new ClarificationResponse
        {
            IssueId = titleIssue.IssueId,
            Outcome = ClarificationOutcome.Clarification,
            Response = "Updated title from QA",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.True);
        Assert.That(result.IsBlocked, Is.False);
        Assert.That(result.UpdatedIntent.Title, Is.EqualTo("Updated title from QA"));
        Assert.That(result.ValidationResult.Status, Is.EqualTo(ValidationStatus.Valid));
        Assert.That(result.UpdatedIntent.ClarificationHistory.Count, Is.EqualTo(1));
    }

    [Test]
    public void ApprovedAssumption_IsRecorded()
    {
        var intent = CreateValidIntent();
        intent.TestData = null!;
        var validation = _validator.Validate(intent);
        var issue = validation.Issues.Single(i => i.Field == "testData");

        var response = new ClarificationResponse
        {
            IssueId = issue.IssueId,
            Outcome = ClarificationOutcome.ApprovedAssumption,
            Response = "{\"userRole\":\"Standard User\"}",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.True);
        Assert.That(result.UpdatedIntent.ApprovedAssumptions.Count, Is.EqualTo(1));
        Assert.That(result.UpdatedIntent.ApprovedAssumptions[0].IssueId, Is.EqualTo(issue.IssueId));
        Assert.That(result.UpdatedIntent.ClarificationHistory.Count, Is.EqualTo(1));
        Assert.That(result.ValidationResult.Status, Is.EqualTo(ValidationStatus.Valid));
    }

    [Test]
    public void RejectedAssumption_LeavesTestIntentUnchanged()
    {
        var intent = CreateValidIntent();
        intent.Title = string.Empty;
        var validation = _validator.Validate(intent);
        var issue = validation.Issues.Single(i => i.Field == "title");
        var originalTitle = intent.Title;

        var response = new ClarificationResponse
        {
            IssueId = issue.IssueId,
            Outcome = ClarificationOutcome.RejectedAssumption,
            Response = "Do not change.",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.True);
        Assert.That(result.IsBlocked, Is.True);
        Assert.That(result.UpdatedIntent.Title, Is.EqualTo(originalTitle));
        Assert.That(result.UpdatedIntent.ClarificationHistory.Count, Is.EqualTo(1));
        Assert.That(result.ValidationResult.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
    }

    [Test]
    public void UnknownIssueId_IsRejected()
    {
        var intent = CreateValidIntent();
        var validation = _validator.Validate(intent);

        var response = new ClarificationResponse
        {
            IssueId = "ISSUE-999",
            Outcome = ClarificationOutcome.Clarification,
            Response = "Anything",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.False);
        Assert.That(result.IsBlocked, Is.True);
        Assert.That(result.RejectionReason, Does.Contain("Unknown issueId"));
    }

    [Test]
    public void ClarificationAttemptingToModifyUnrelatedField_IsRejected()
    {
        var intent = CreateValidIntent();
        intent.ExpectedResults = [];
        var validation = _validator.Validate(intent);
        var issue = validation.Issues.Single(i => i.Field == "expectedResults");

        var response = new ClarificationResponse
        {
            IssueId = issue.IssueId,
            Outcome = ClarificationOutcome.Clarification,
            Response = "{\"title\":\"Injected title\"}",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.False);
        Assert.That(result.IsBlocked, Is.True);
        Assert.That(result.RejectionReason, Does.Contain("Invalid value for field 'expectedResults'"));
        Assert.That(result.UpdatedIntent.Title, Is.EqualTo("Successful login"));
    }

    [Test]
    public void ClarificationApplied_FullValidationRunsAgain()
    {
        var intent = CreateValidIntent();
        intent.Title = string.Empty;
        intent.ExpectedResults = [];
        var validation = _validator.Validate(intent);
        var titleIssue = validation.Issues.Single(i => i.Field == "title");

        var response = new ClarificationResponse
        {
            IssueId = titleIssue.IssueId,
            Outcome = ClarificationOutcome.Clarification,
            Response = "Title from QA",
            RespondedBy = "QA_ENGINEER"
        };

        var result = _revalidationService.Revalidate(intent, validation, response);

        Assert.That(result.IsAccepted, Is.True);
        Assert.That(result.IsBlocked, Is.True);
        Assert.That(result.ValidationResult.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.ValidationResult.Issues.Any(i => i.Field == "expectedResults"), Is.True);
    }

    private static TestIntent CreateValidIntent() =>
        new()
        {
            TestId = "TC-001",
            Title = "Successful login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User is on Login page"],
            TestData = new Dictionary<string, object?> { ["username"] = "u", ["password"] = "p" },
            Actions =
            [
                new TestAction { Step = 1, Action = "Enter username" },
                new TestAction { Step = 2, Action = "Enter password" },
                new TestAction { Step = 3, Action = "Click login" }
            ],
            ExpectedResults = ["Dashboard page is displayed"]
        };
}
