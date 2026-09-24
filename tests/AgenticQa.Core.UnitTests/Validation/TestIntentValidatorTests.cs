using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using AgenticQa.Core.Validation;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Validation;

public class TestIntentValidatorTests
{
    private TestIntentValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new TestIntentValidator(NullLogger<TestIntentValidator>.Instance);
    }

    [Test]
    public void ValidTestIntent_ReturnsValid()
    {
        var intent = CreateValidIntent();

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.Valid));
        Assert.That(result.Issues, Is.Empty);
    }

    [Test]
    public void MissingTitle_ReturnsNeedsClarification()
    {
        var intent = CreateValidIntent();
        intent.Title = " ";

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(i => i.Field == "title" && i.Type == ClarificationIssueType.Missing), Is.True);
    }

    [Test]
    public void NoActions_ReturnsNeedsClarification()
    {
        var intent = CreateValidIntent();
        intent.Actions = [];

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(i => i.Field == "actions" && i.Type == ClarificationIssueType.Missing), Is.True);
    }

    [Test]
    public void NoExpectedResults_ReturnsNeedsClarification()
    {
        var intent = CreateValidIntent();
        intent.ExpectedResults = [];

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(i => i.Field == "expectedResults" && i.Type == ClarificationIssueType.Missing), Is.True);
    }

    [Test]
    public void NonSequentialActionNumbers_ReturnsNeedsClarification()
    {
        var intent = CreateValidIntent();
        intent.Actions =
        [
            new TestAction { Step = 1, Action = "Enter username" },
            new TestAction { Step = 3, Action = "Click login" }
        ];

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(i => i.Field == "actions" && i.Type == ClarificationIssueType.Conflicting), Is.True);
    }

    [Test]
    public void DuplicateActionNumbers_ReturnsNeedsClarification()
    {
        var intent = CreateValidIntent();
        intent.Actions =
        [
            new TestAction { Step = 1, Action = "Enter username" },
            new TestAction { Step = 1, Action = "Click login" }
        ];

        var result = _validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(i => i.Message.Contains("Duplicate action step number", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void DeterministicIssueIds_AreStable()
    {
        var intent = CreateValidIntent();
        intent.Title = " ";
        intent.ExpectedResults = [];

        var result = _validator.Validate(intent);

        Assert.That(result.Issues.Select(x => x.IssueId), Is.EqualTo(["ISSUE-001", "ISSUE-002"]));
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
