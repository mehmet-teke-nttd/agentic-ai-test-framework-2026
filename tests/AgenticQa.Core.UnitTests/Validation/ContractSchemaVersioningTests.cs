using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Validation;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Validation;

public class ContractSchemaVersioningTests
{
    [Test]
    public void TestIntentValidator_MigratesLegacyVersionToCurrent()
    {
        var validator = new TestIntentValidator(NullLogger<TestIntentValidator>.Instance);
        var intent = CreateValidIntent();
        intent.SchemaVersion = ContractSchemaVersions.Legacy;

        var result = validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.Valid));
        Assert.That(intent.SchemaVersion, Is.EqualTo(ContractSchemaVersions.Current));
    }

    [Test]
    public void TestIntentValidator_UnsupportedSchemaVersion_NeedsClarification()
    {
        var validator = new TestIntentValidator(NullLogger<TestIntentValidator>.Instance);
        var intent = CreateValidIntent();
        intent.SchemaVersion = "2.0";

        var result = validator.Validate(intent);

        Assert.That(result.Status, Is.EqualTo(ValidationStatus.NeedsClarification));
        Assert.That(result.Issues.Any(issue => issue.Field == "schemaVersion"), Is.True);
    }

    [Test]
    public void ExecutionContractValidator_UnsupportedSchemaVersion_ReturnsError()
    {
        var stepValidator = Substitute.For<IExecutionStepValidator>();
        stepValidator.Validate(Arg.Any<ExecutionStep>()).Returns(new ExecutionStepValidationResult
        {
            IsValid = true,
            Errors = []
        });
        var dataValidator = Substitute.For<ITestDataReferenceValidator>();
        dataValidator.Validate(Arg.Any<ExecutionStep>(), Arg.Any<IReadOnlyDictionary<string, object?>>()).Returns([]);
        var validator = new ExecutionContractValidator(
            NullLogger<ExecutionContractValidator>.Instance,
            stepValidator,
            dataValidator);

        var intent = CreateValidIntent();
        var contract = new ExecutionContract
        {
            SchemaVersion = "2.0",
            TestId = intent.TestId,
            ExecutionType = "UI",
            Steps = [new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" }]
        };

        var result = validator.Validate(contract, intent);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.ErrorCode == ExecutionContractValidationErrorCode.UnsupportedSchemaVersion), Is.True);
    }

    private static TestIntent CreateValidIntent() =>
        new()
        {
            TestId = "TC-001",
            Title = "Valid login test",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User exists"],
            TestData = new Dictionary<string, object?> { ["username"] = "u", ["password"] = "p" },
            Actions = [new TestAction { Step = 1, Action = "Navigate to Login page" }],
            ExpectedResults = ["User lands on login page"]
        };
}
