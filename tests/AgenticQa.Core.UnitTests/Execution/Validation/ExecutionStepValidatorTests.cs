using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Validation;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.UnitTests.Execution.Validation;

public class ExecutionStepValidatorTests
{
    private ExecutionStepValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ExecutionStepValidator();
    }

    [Test]
    public void FillWithoutTarget_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.Fill,
            Value = "{{username}}"
        };

        var result = _validator.Validate(step);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.MissingTarget), Is.True);
    }

    [Test]
    public void FillWithoutValue_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.Fill,
            Target = "UsernameInput"
        };

        var result = _validator.Validate(step);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.MissingValue), Is.True);
    }

    [Test]
    public void VerifyTextWithoutExpected_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.VerifyText,
            Target = "WelcomeMessage"
        };

        var result = _validator.Validate(step);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.MissingExpected), Is.True);
    }

    [Test]
    public void VerifyUrlWithoutExpected_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = ExecutionKeyword.VerifyUrl
        };

        var result = _validator.Validate(step);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.MissingExpected), Is.True);
    }

    [Test]
    public void UnknownKeyword_IsInvalid()
    {
        var step = new ExecutionStep
        {
            Step = 1,
            Keyword = (ExecutionKeyword)999,
            Target = "AnyTarget"
        };

        var result = _validator.Validate(step);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ExecutionContractValidationErrorCode.UnsupportedKeyword), Is.True);
    }
}
