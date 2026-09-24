using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Validation;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Validation;

public class ExecutionContractValidatorTests
{
    private ExecutionContractValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ExecutionContractValidator(
            NullLogger<ExecutionContractValidator>.Instance,
            new ExecutionStepValidator(),
            new TestDataReferenceValidator(NullLogger<TestDataReferenceValidator>.Instance));
    }

    [Test]
    public void ValidExecutionContract_IsValid()
    {
        var contract = CreateValidContract();
        var intent = CreateIntentWithTestData();

        var result = _validator.Validate(contract, intent);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void EmptyTestId_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.TestId = string.Empty;

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.MissingTestId), Is.True);
    }

    [Test]
    public void NoSteps_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.Steps = [];

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.NoExecutionSteps), Is.True);
    }

    [Test]
    public void StepNumberingStartsAtTwo_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.Steps =
        [
            new ExecutionStep { Step = 2, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" }
        ];

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.NonSequentialSteps), Is.True);
    }

    [Test]
    public void DuplicateStepNumbers_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.Steps =
        [
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" },
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" }
        ];

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.DuplicateStepNumber), Is.True);
    }

    [Test]
    public void NonSequentialSteps_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.Steps =
        [
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" },
            new ExecutionStep { Step = 3, Keyword = ExecutionKeyword.Click, Target = "LoginButton" }
        ];

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.NonSequentialSteps), Is.True);
    }

    [Test]
    public void UnknownKeyword_IsInvalid()
    {
        var contract = CreateValidContract();
        contract.Steps[1].Keyword = (ExecutionKeyword)999;

        var result = _validator.Validate(contract, CreateIntentWithTestData());

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.UnsupportedKeyword), Is.True);
    }

    private static ExecutionContract CreateValidContract() =>
        new()
        {
            TestId = "TC-001",
            ExecutionType = "UI",
            Steps =
            [
                new ExecutionStep
                {
                    Step = 1,
                    Keyword = ExecutionKeyword.Navigate,
                    Target = "LoginPage"
                },
                new ExecutionStep
                {
                    Step = 2,
                    Keyword = ExecutionKeyword.Fill,
                    Target = "UsernameInput",
                    Value = "{{username}}"
                },
                new ExecutionStep
                {
                    Step = 3,
                    Keyword = ExecutionKeyword.VerifyUrl,
                    Expected = "/dashboard"
                }
            ]
        };

    private static TestIntent CreateIntentWithTestData() =>
        new()
        {
            TestId = "TC-001",
            Title = "Login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User on login page"],
            TestData = new Dictionary<string, object?>
            {
                ["username"] = "validUser",
                ["password"] = "validPassword"
            },
            Actions = [new TestAction { Step = 1, Action = "Enter username" }],
            ExpectedResults = ["Dashboard is visible"]
        };
}
