using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class ExecutionEngineTests
{
    private ITestIntentValidator _intentValidator = null!;
    private IExecutionContractValidator _contractValidator = null!;
    private IStepExecutor _stepExecutor = null!;
    private ITestResultWriter _writer = null!;
    private ExecutionEngine _engine = null!;

    [SetUp]
    public void Setup()
    {
        _intentValidator = Substitute.For<ITestIntentValidator>();
        _contractValidator = Substitute.For<IExecutionContractValidator>();
        _stepExecutor = Substitute.For<IStepExecutor>();
        _writer = Substitute.For<ITestResultWriter>();
        _writer.WriteAsync(Arg.Any<TestExecutionResult>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("dummy.json"));

        _engine = new ExecutionEngine(
            NullLogger<ExecutionEngine>.Instance,
            _intentValidator,
            _contractValidator,
            _stepExecutor,
            new TestResultAggregator(NullLogger<TestResultAggregator>.Instance),
            _writer);

        _intentValidator.Validate(Arg.Any<TestIntent>()).Returns(new TestIntentValidationResult
        {
            Status = ValidationStatus.Valid,
            Issues = []
        });
        _contractValidator.Validate(Arg.Any<ExecutionContract>(), Arg.Any<TestIntent>())
            .Returns(new ExecutionContractValidationResult
            {
                IsValid = true,
                Errors = []
            });
    }

    [Test]
    public async Task AllStepsPassed_OverallPassed()
    {
        var contract = CreateContract(3);
        var context = CreateContext();
        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call => Passed((ExecutionStep)call[0], context));

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(result.PassedSteps, Is.EqualTo(3));
        Assert.That(result.StepResults.Select(x => x.Step), Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task FailedStep_CausesLaterSkipped_OverallFailed()
    {
        var contract = CreateContract(5);
        var context = CreateContext();
        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var step = (ExecutionStep)call[0];
                return step.Step == 3 ? Failed(step, context) : Passed(step, context);
            });

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(result.FailedSteps, Is.EqualTo(1));
        Assert.That(result.SkippedSteps, Is.EqualTo(2));
        Assert.That(result.StepResults[3].SkipReason, Is.EqualTo(SkipReason.PreviousStepFailed));
    }

    [Test]
    public async Task BlockedStep_CausesLaterSkipped_OverallBlocked()
    {
        var contract = CreateContract(4);
        var context = CreateContext();
        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var step = (ExecutionStep)call[0];
                return step.Step == 2 ? Blocked(step, context) : Passed(step, context);
            });

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
        Assert.That(result.BlockedSteps, Is.EqualTo(1));
        Assert.That(result.SkippedSteps, Is.EqualTo(2));
        Assert.That(result.StepResults[2].SkipReason, Is.EqualTo(SkipReason.PreviousStepBlocked));
    }

    [Test]
    public async Task InvalidExecutionContract_BlocksWithoutRunningSteps()
    {
        _contractValidator.Validate(Arg.Any<ExecutionContract>(), Arg.Any<TestIntent>())
            .Returns(new ExecutionContractValidationResult
            {
                IsValid = false,
                Errors =
                [
                    new ExecutionContractValidationError
                    {
                        ErrorCode = ExecutionContractValidationErrorCode.NoExecutionSteps,
                        Message = "invalid"
                    }
                ]
            });

        var result = await _engine.ExecuteAsync(CreateContract(2), CreateContext());

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("EXECUTION_CONTRACT_INVALID"));
        await _stepExecutor.DidNotReceiveWithAnyArgs()
            .ExecuteAsync(default!, default!, default);
    }

    [Test]
    public async Task InvalidTestIntent_BlocksWithoutRunningSteps()
    {
        _intentValidator.Validate(Arg.Any<TestIntent>()).Returns(new TestIntentValidationResult
        {
            Status = ValidationStatus.NeedsClarification,
            Issues =
            [
                new ClarificationIssue
                {
                    IssueId = "ISSUE-001",
                    Field = "title",
                    Type = ClarificationIssueType.Missing,
                    Message = "title required"
                }
            ]
        });

        var result = await _engine.ExecuteAsync(CreateContract(2), CreateContext());

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("TEST_INTENT_INVALID"));
        await _stepExecutor.DidNotReceiveWithAnyArgs()
            .ExecuteAsync(default!, default!, default);
    }

    [Test]
    public async Task Cancellation_StopsExecution_AndSkipsRemaining()
    {
        var contract = CreateContract(4);
        var context = CreateContext();
        using var cts = new CancellationTokenSource();
        var executionCount = 0;

        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                executionCount++;
                if (executionCount == 1)
                {
                    cts.Cancel();
                }
                return Passed((ExecutionStep)call[0], context);
            });

        var result = await _engine.ExecuteAsync(contract, context, cts.Token);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("EXECUTION_CANCELLED"));
        Assert.That(result.SkippedSteps, Is.EqualTo(3));
    }

    [Test]
    public async Task DurationAndOrder_ArePopulated()
    {
        var contract = CreateContract(3);
        var context = CreateContext();
        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call => Passed((ExecutionStep)call[0], context));

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.DurationMs, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.StartedAt, Is.Not.EqualTo(default(DateTimeOffset)));
        Assert.That(result.CompletedAt, Is.Not.EqualTo(default(DateTimeOffset)));
        Assert.That(result.StepResults.Select(x => x.Step), Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task IntegrationStyle_LoginFlow_AllPassed()
    {
        var context = CreateContext();
        var contract = new ExecutionContract
        {
            TestId = "TC-LOGIN-1",
            ExecutionType = "UI",
            Steps =
            [
                new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" },
                new ExecutionStep { Step = 2, Keyword = ExecutionKeyword.Fill, Target = "UsernameInput", Value = "{{username}}" },
                new ExecutionStep { Step = 3, Keyword = ExecutionKeyword.Fill, Target = "PasswordInput", Value = "{{password}}" },
                new ExecutionStep { Step = 4, Keyword = ExecutionKeyword.Click, Target = "LoginButton" },
                new ExecutionStep { Step = 5, Keyword = ExecutionKeyword.VerifyVisible, Target = "DashboardPage" }
            ]
        };

        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call => Passed((ExecutionStep)call[0], context));

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(result.PassedSteps, Is.EqualTo(5));
    }

    [Test]
    public async Task IntegrationStyle_VerifyVisibleFails_RemainingSkipped()
    {
        var context = CreateContext();
        var contract = new ExecutionContract
        {
            TestId = "TC-LOGIN-2",
            ExecutionType = "UI",
            Steps =
            [
                new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" },
                new ExecutionStep { Step = 2, Keyword = ExecutionKeyword.Fill, Target = "UsernameInput", Value = "{{username}}" },
                new ExecutionStep { Step = 3, Keyword = ExecutionKeyword.Fill, Target = "PasswordInput", Value = "{{password}}" },
                new ExecutionStep { Step = 4, Keyword = ExecutionKeyword.VerifyVisible, Target = "DashboardPage" },
                new ExecutionStep { Step = 5, Keyword = ExecutionKeyword.VerifyUrl, Expected = "/dashboard" }
            ]
        };

        _stepExecutor.ExecuteAsync(Arg.Any<ExecutionStep>(), context, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var step = (ExecutionStep)call[0];
                return step.Step == 4 ? Failed(step, context) : Passed(step, context);
            });

        var result = await _engine.ExecuteAsync(contract, context);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(result.FailedSteps, Is.EqualTo(1));
        Assert.That(result.SkippedSteps, Is.EqualTo(1));
        Assert.That(result.StepResults.Last().Status, Is.EqualTo(StepStatus.Skipped));
    }

    private static StepExecutionResult Passed(ExecutionStep step, ExecutionContext context) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Passed
        };

    private static StepExecutionResult Failed(ExecutionStep step, ExecutionContext context) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Failed,
            Error = new ExecutionError
            {
                ErrorCode = "ASSERTION_FAILED",
                Message = "failed"
            }
        };

    private static StepExecutionResult Blocked(ExecutionStep step, ExecutionContext context) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Blocked,
            Error = new ExecutionError
            {
                ErrorCode = "LOCATOR_NOT_FOUND",
                Message = "blocked"
            }
        };

    private static ExecutionContract CreateContract(int count) =>
        new()
        {
            TestId = "TC-001",
            ExecutionType = "UI",
            Steps = Enumerable.Range(1, count)
                .Select(step => new ExecutionStep
                {
                    Step = step,
                    Keyword = ExecutionKeyword.Click,
                    Target = $"Button{step}"
                })
                .ToList()
        };

    private static ExecutionContext CreateContext() =>
        new()
        {
            Page = Substitute.For<IPage>(),
            CurrentPage = "LoginPage",
            TestIntent = new TestIntent
            {
                TestId = "TC-001",
                Title = "Login",
                TestType = TestType.UiFunctionalPositive,
                Preconditions = ["User on page"],
                TestData = new Dictionary<string, object?> { ["username"] = "u", ["password"] = "p" },
                Actions = [new TestAction { Step = 1, Action = "Click Login button" }],
                ExpectedResults = ["success"]
            }
        };
}
