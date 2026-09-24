using AgenticQa.BddTests.Support;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Reqnroll;

namespace AgenticQa.BddTests.StepDefinitions;

[Binding]
public sealed class LoginStepDefinitions
{
    private readonly QaScenarioContext _scenarioContext;
    private readonly BrowserSessionContext _browserSessionContext;
    private readonly ITestIntentValidator _testIntentValidator;
    private readonly IExecutionContractMapper _executionContractMapper;
    private readonly IExecutionContractValidator _executionContractValidator;
    private readonly IExecutionEngine _executionEngine;
    private readonly IFailureClassificationService _failureClassificationService;
    private readonly IFailureAnalysisHistoryService _failureAnalysisHistoryService;

    public LoginStepDefinitions(
        QaScenarioContext scenarioContext,
        BrowserSessionContext browserSessionContext,
        ITestIntentValidator testIntentValidator,
        IExecutionContractMapper executionContractMapper,
        IExecutionContractValidator executionContractValidator,
        IExecutionEngine executionEngine,
        IFailureClassificationService failureClassificationService,
        IFailureAnalysisHistoryService failureAnalysisHistoryService)
    {
        _scenarioContext = scenarioContext;
        _browserSessionContext = browserSessionContext;
        _testIntentValidator = testIntentValidator;
        _executionContractMapper = executionContractMapper;
        _executionContractValidator = executionContractValidator;
        _executionEngine = executionEngine;
        _failureClassificationService = failureClassificationService;
        _failureAnalysisHistoryService = failureAnalysisHistoryService;
    }

    [Given("the user has a valid login test intent")]
    public void GivenTheUserHasAValidLoginTestIntent()
    {
        _scenarioContext.TestIntent = LoginTestIntentFactory.Create();
    }

    [When("the test intent is validated")]
    public void WhenTheTestIntentIsValidated()
    {
        Assert.That(_scenarioContext.TestIntent, Is.Not.Null);
        _scenarioContext.ValidationResult = _testIntentValidator.Validate(_scenarioContext.TestIntent!);
        Assert.That(_scenarioContext.ValidationResult.Status, Is.EqualTo(ValidationStatus.Valid));
    }

    [When("the execution contract is generated")]
    public void WhenTheExecutionContractIsGenerated()
    {
        Assert.That(_scenarioContext.TestIntent, Is.Not.Null);
        var mapResult = _executionContractMapper.Map(_scenarioContext.TestIntent!);
        Assert.That(mapResult.Status, Is.EqualTo(MappingStatus.Success), "Execution contract mapping failed.");
        Assert.That(mapResult.ExecutionContract, Is.Not.Null);

        _scenarioContext.ExecutionContract = mapResult.ExecutionContract;
    }

    [When("the execution contract is configured to force a verification failure")]
    public void WhenTheExecutionContractIsConfiguredToForceAVerificationFailure()
    {
        Assert.That(_scenarioContext.ExecutionContract, Is.Not.Null);
        var stepToFail = _scenarioContext.ExecutionContract!.Steps.Single(step => step.Step == 5);
        stepToFail.Keyword = ExecutionKeyword.VerifyText;
        stepToFail.Target = "WelcomeMessage";
        stepToFail.Expected = "Wrong Welcome";

        _scenarioContext.ExecutionContract.Steps.Add(new ExecutionStep
        {
            Step = 6,
            Keyword = ExecutionKeyword.VerifyUrl,
            Expected = "https://invalid/url"
        });
    }

    [When("the execution contract is configured with an unregistered target")]
    public void WhenTheExecutionContractIsConfiguredWithAnUnregisteredTarget()
    {
        Assert.That(_scenarioContext.ExecutionContract, Is.Not.Null);
        var clickStep = _scenarioContext.ExecutionContract!.Steps.Single(step => step.Keyword == ExecutionKeyword.Click);
        clickStep.Target = "UnknownLoginButton";
    }

    [When("the execution contract is executed")]
    public async Task WhenTheExecutionContractIsExecutedAsync()
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");
        Assert.That(_scenarioContext.ExecutionContract, Is.Not.Null);
        Assert.That(_scenarioContext.TestIntent, Is.Not.Null);

        _scenarioContext.ExecutionContractValidationResult = _executionContractValidator.Validate(
            _scenarioContext.ExecutionContract!,
            _scenarioContext.TestIntent!);
        Assert.That(_scenarioContext.ExecutionContractValidationResult.IsValid, Is.True, "Execution contract validation failed.");

        var executionContext = new ExecutionContext
        {
            Page = _browserSessionContext.Page!,
            TestIntent = _scenarioContext.TestIntent!
        };

        _scenarioContext.TestExecutionResult = await _executionEngine.ExecuteAsync(
            _scenarioContext.ExecutionContract!,
            executionContext);
        _scenarioContext.ResultFilePath = Path.Combine(
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "artifacts", "results")),
            $"{_scenarioContext.TestExecutionResult.TestId}-result.json");

        _scenarioContext.FailureAnalysisResult = await _failureClassificationService.ClassifyIfNeededAsync(
            _scenarioContext.TestIntent!,
            _scenarioContext.ExecutionContract!,
            _scenarioContext.TestExecutionResult,
            executionContext.CurrentPage,
            null,
            []);

        if (_scenarioContext.FailureAnalysisResult is not null)
        {
            _scenarioContext.FailureAnalysisPath = await _failureAnalysisHistoryService.SaveAnalysisAsync(
                _scenarioContext.FailureAnalysisResult);
        }
    }

    [Then("the overall test result should be (PASSED|FAILED|BLOCKED)")]
    public void ThenTheOverallTestResultShouldBe(string status)
    {
        Assert.That(_scenarioContext.TestExecutionResult, Is.Not.Null);
        var expected = Enum.Parse<TestStatus>(status, true);
        var actual = _scenarioContext.TestExecutionResult!.Status;

        Assert.That(actual, Is.EqualTo(expected), BuildDiagnosticMessage(_scenarioContext.TestExecutionResult));
    }

    [Then("dependent steps should be skipped")]
    public void ThenDependentStepsShouldBeSkipped()
    {
        Assert.That(_scenarioContext.TestExecutionResult, Is.Not.Null);
        Assert.That(_scenarioContext.TestExecutionResult!.SkippedSteps, Is.GreaterThan(0));
    }

    [Then("a result file should be written")]
    public void ThenAResultFileShouldBeWritten()
    {
        Assert.That(_scenarioContext.ResultFilePath, Is.Not.Null.And.Not.Empty);
        Assert.That(File.Exists(_scenarioContext.ResultFilePath!), Is.True);
    }

    private static string BuildDiagnosticMessage(TestExecutionResult result)
    {
        var failedStep = result.StepResults
            .FirstOrDefault(step => step.Status is StepStatus.Failed or StepStatus.Blocked);

        var errorCode = failedStep?.Error?.ErrorCode ?? result.Error?.ErrorCode ?? "N/A";
        var errorMessage = failedStep?.Error?.Message ?? result.Error?.Message ?? "N/A";

        return $"Status={result.Status}, FailedStep={failedStep?.Step}, ErrorCode={errorCode}, ErrorMessage={errorMessage}";
    }
}
