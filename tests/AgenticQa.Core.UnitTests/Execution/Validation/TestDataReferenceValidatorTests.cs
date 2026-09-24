using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Validation;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Validation;

public class TestDataReferenceValidatorTests
{
    private TestDataReferenceValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new TestDataReferenceValidator(NullLogger<TestDataReferenceValidator>.Instance);
    }

    [Test]
    public void KnownTestDataVariable_IsValid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.Fill,
            Target = "UsernameInput",
            Value = "{{username}}"
        };

        var testData = new Dictionary<string, object?> { ["username"] = "validUser" };
        var errors = _validator.Validate(step, testData);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void UnknownTestDataVariable_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.Fill,
            Target = "UsernameInput",
            Value = "{{username}}"
        };

        var testData = new Dictionary<string, object?>();
        var errors = _validator.Validate(step, testData);

        Assert.That(errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.TestDataReferenceNotFound), Is.True);
    }

    [Test]
    public void LiteralValue_IsValid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.VerifyUrl,
            Expected = "https://app.local/dashboard"
        };

        var testData = new Dictionary<string, object?>();
        var errors = _validator.Validate(step, testData);

        Assert.That(errors, Is.Empty);
    }
}
