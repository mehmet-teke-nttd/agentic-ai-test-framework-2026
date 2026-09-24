using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class TestDataResolverTests
{
    private TestDataResolver _resolver = null!;
    private TestIntent _intent = null!;

    [SetUp]
    public void Setup()
    {
        _resolver = new TestDataResolver();
        _intent = new TestIntent
        {
            TestId = "TC-001",
            Title = "Login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["precondition"],
            TestData = new Dictionary<string, object?> { ["username"] = "validUser" },
            Actions = [new TestAction { Step = 1, Action = "Enter username" }],
            ExpectedResults = ["Success"]
        };
    }

    [Test]
    public void LiteralValue_ReturnsLiteral()
    {
        var result = _resolver.Resolve("literal-value", _intent);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ResolvedValue, Is.EqualTo("literal-value"));
    }

    [Test]
    public void PlaceholderValue_ResolvesFromTestData()
    {
        var result = _resolver.Resolve("{{username}}", _intent);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ResolvedValue, Is.EqualTo("validUser"));
    }

    [Test]
    public void MissingPlaceholder_ReturnsError()
    {
        var result = _resolver.Resolve("{{password}}", _intent);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo(RuntimeExecutionErrorCode.TestDataReferenceNotFound));
    }
}
