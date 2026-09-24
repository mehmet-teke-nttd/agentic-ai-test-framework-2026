using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugDraftBuilderTests
{
    private BugDraftBuilder _builder = null!;

    [SetUp]
    public void Setup()
    {
        _builder = new BugDraftBuilder(NullLogger<BugDraftBuilder>.Instance);
    }

    [Test]
    public void Builder_UsesIntentActions_ExpectedAndActual()
    {
        var intent = new TestIntent
        {
            TestId = "TC-LOGIN-001",
            Title = "Valid login does not display Dashboard",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User exists"],
            TestData = [],
            Actions =
            [
                new TestAction { Step = 1, Action = "Navigate to Login page." },
                new TestAction { Step = 2, Action = "Enter valid username." }
            ],
            ExpectedResults = ["Dashboard should be displayed."]
        };

        var executionContract = new ExecutionContract
        {
            TestId = "TC-LOGIN-001",
            ExecutionType = "UI",
            Steps = [new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" }]
        };

        var executionResult = new TestExecutionResult
        {
            TestId = "TC-LOGIN-001",
            Status = TestStatus.Failed,
            StepResults =
            [
                new StepExecutionResult
                {
                    TestId = "TC-LOGIN-001",
                    Step = 5,
                    Keyword = ExecutionKeyword.VerifyVisible,
                    Target = "DashboardPage",
                    Status = StepStatus.Failed,
                    Error = new ExecutionError
                    {
                        ErrorCode = "ASSERTION_FAILED",
                        Message = "Dashboard was not displayed."
                    }
                }
            ]
        };

        var failureAnalysis = new FailureAnalysisResult
        {
            TestId = "TC-LOGIN-001",
            Classification = FailureClassification.ProductDefect,
            Confidence = 0.87,
            Summary = "Login completed but dashboard was not shown.",
            Evidence = ["Steps 1-4 passed", "VERIFY_VISIBLE failed"],
            RecommendedAction = FailureRecommendedAction.QaReviewRequired
        };

        var draft = _builder.Build(intent, executionContract, executionResult, failureAnalysis);

        Assert.That(draft.StepsToReproduce, Is.EqualTo(intent.Actions.Select(a => a.Action).ToList()));
        Assert.That(draft.ExpectedResult, Is.EqualTo("Dashboard should be displayed."));
        Assert.That(draft.ActualResult, Does.Contain("Step 5"));
        Assert.That(draft.Priority, Is.EqualTo(BugPriority.Unassigned));
    }
}
