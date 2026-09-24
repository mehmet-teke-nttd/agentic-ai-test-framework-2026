using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugRecommendationServiceTests
{
    private BugRecommendationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new BugRecommendationService(NullLogger<BugRecommendationService>.Instance);
    }

    [Test]
    public void ConfirmedProductDefect_Recommended()
    {
        var result = _service.Recommend(
            CreateIntent(),
            CreateExecutionResult(),
            CreateFailureAnalysis(FailureClassification.ProductDefect),
            FailureReviewDecision.Confirmed);

        Assert.That(result.Status, Is.EqualTo(BugRecommendationStatus.BugDraftRecommended));
    }

    [Test]
    public void ProductDefectNeedsInvestigation_NoBugYet()
    {
        var result = _service.Recommend(
            CreateIntent(),
            CreateExecutionResult(),
            CreateFailureAnalysis(FailureClassification.ProductDefect),
            FailureReviewDecision.NeedsMoreInvestigation);

        Assert.That(result.Status, Is.EqualTo(BugRecommendationStatus.NoBugYet));
    }

    [Test]
    public void RejectedProductDefect_NoBug()
    {
        var result = _service.Recommend(
            CreateIntent(),
            CreateExecutionResult(),
            CreateFailureAnalysis(FailureClassification.ProductDefect),
            FailureReviewDecision.Rejected);

        Assert.That(result.Status, Is.EqualTo(BugRecommendationStatus.NoBug));
    }

    [TestCase(FailureClassification.AutomationIssue)]
    [TestCase(FailureClassification.TestDataIssue)]
    [TestCase(FailureClassification.EnvironmentIssue)]
    [TestCase(FailureClassification.RequirementIssue)]
    [TestCase(FailureClassification.Unknown)]
    public void NonProductDefect_NoBug(FailureClassification classification)
    {
        var result = _service.Recommend(
            CreateIntent(),
            CreateExecutionResult(),
            CreateFailureAnalysis(classification),
            FailureReviewDecision.Confirmed);

        Assert.That(result.Status, Is.EqualTo(BugRecommendationStatus.NoBug));
    }

    private static TestIntent CreateIntent() =>
        new()
        {
            TestId = "TC-1",
            Title = "Login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["pre"],
            TestData = [],
            Actions = [new TestAction { Step = 1, Action = "Click login" }],
            ExpectedResults = ["Dashboard displayed"]
        };

    private static TestExecutionResult CreateExecutionResult() =>
        new()
        {
            TestId = "TC-1",
            Status = TestStatus.Failed
        };

    private static FailureAnalysisResult CreateFailureAnalysis(FailureClassification classification) =>
        new()
        {
            TestId = "TC-1",
            Classification = classification,
            Confidence = 0.9,
            Summary = "summary",
            Evidence = ["e1"],
            RecommendedAction = FailureRecommendedAction.QaReviewRequired
        };
}
