using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Mapping;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Mapping;

public class ExecutionContractMapperTests
{
    private ExecutionContractMapper _mapper = null!;

    [SetUp]
    public void Setup()
    {
        _mapper = new ExecutionContractMapper(NullLogger<ExecutionContractMapper>.Instance);
    }

    [Test]
    public void KnownActions_MapCorrectly()
    {
        var intent = new TestIntent
        {
            TestId = "TC-001",
            Title = "Successful login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User is on Login page"],
            TestData = new Dictionary<string, object?> { ["username"] = "u", ["password"] = "p" },
            Actions =
            [
                new TestAction { Step = 1, Action = "Navigate to Login page" },
                new TestAction { Step = 2, Action = "Enter username" },
                new TestAction { Step = 3, Action = "Enter password" },
                new TestAction { Step = 4, Action = "Click Login button" },
                new TestAction { Step = 5, Action = "Verify Dashboard is visible" }
            ],
            ExpectedResults = ["Dashboard displayed"]
        };

        var result = _mapper.Map(intent);

        Assert.That(result.Status, Is.EqualTo(MappingStatus.Success));
        Assert.That(result.ExecutionContract, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ExecutionContract!.Steps.Count, Is.EqualTo(5));
            Assert.That(result.ExecutionContract.SchemaVersion, Is.EqualTo(ContractSchemaVersions.Current));
            Assert.That(result.ExecutionContract.ExecutionType, Is.EqualTo("UI"));
            Assert.That(result.ExecutionContract.Steps[0].Keyword, Is.EqualTo(ExecutionKeyword.Navigate));
            Assert.That(result.ExecutionContract.Steps[1].Keyword, Is.EqualTo(ExecutionKeyword.Fill));
            Assert.That(result.ExecutionContract.Steps[1].Target, Is.EqualTo("AuthForm.UsernameInput"));
            Assert.That(result.ExecutionContract.Steps[1].Value, Is.EqualTo("{{username}}"));
            Assert.That(result.ExecutionContract.Steps[4].Keyword, Is.EqualTo(ExecutionKeyword.VerifyVisible));
        });
    }

    [Test]
    public void UnknownAction_ReturnsMappingFailed()
    {
        var intent = new TestIntent
        {
            TestId = "TC-001",
            Title = "Checkout test",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User is logged in"],
            TestData = [],
            Actions =
            [
                new TestAction { Step = 1, Action = "Complete the checkout" }
            ],
            ExpectedResults = ["Order is created"]
        };

        var result = _mapper.Map(intent);

        Assert.That(result.Status, Is.EqualTo(MappingStatus.MappingFailed));
        Assert.That(result.ExecutionContract, Is.Null);
        Assert.That(result.Issues.Single().ErrorCode, Is.EqualTo(ExecutionContractValidationErrorCode.ActionMappingNotFound));
    }

    [Test]
    public void Mapper_DoesNotInventUnknownTargets()
    {
        var intent = new TestIntent
        {
            TestId = "TC-001",
            Title = "Unknown action target",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User is on some page"],
            TestData = [],
            Actions =
            [
                new TestAction { Step = 1, Action = "Click the magical proceed button" }
            ],
            ExpectedResults = ["Continue"]
        };

        var result = _mapper.Map(intent);

        Assert.That(result.Status, Is.EqualTo(MappingStatus.MappingFailed));
        Assert.That(result.ExecutionContract, Is.Null);
        Assert.That(result.Issues.Single().Message, Is.EqualTo("No deterministic execution mapping exists for this action."));
    }
}
