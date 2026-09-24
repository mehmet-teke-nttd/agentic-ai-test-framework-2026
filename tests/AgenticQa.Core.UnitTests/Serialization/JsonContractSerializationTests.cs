using System.Text.Json;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using ElementRegistryModel = AgenticQa.Core.Models.ElementRegistry;

namespace AgenticQa.Core.UnitTests.Serialization;

public class JsonContractSerializationTests
{
    [Test]
    public void TestIntent_RoundTrip_PreservesContractShapeAndEnumValues()
    {
        var intent = new TestIntent
        {
            TestId = "TC-001",
            Title = "Successful login with valid credentials",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User is on the Login page", "Valid user account exists"],
            TestData = new Dictionary<string, object?>
            {
                ["username"] = "validUser",
                ["password"] = "validPassword"
            },
            Actions =
            [
                new TestAction { Step = 1, Action = "Enter username" },
                new TestAction { Step = 2, Action = "Enter password" },
                new TestAction { Step = 3, Action = "Click Login button" }
            ],
            ExpectedResults = ["User is authenticated successfully", "Dashboard page is displayed"]
        };

        var json = AgenticJsonSerializer.Serialize(intent);
        Assert.That(json, Does.Contain("\"schemaVersion\": \"1.0\""));
        Assert.That(json, Does.Contain("\"testType\": \"UI-Functional-Positive\""));

        var roundTrip = AgenticJsonSerializer.Deserialize<TestIntent>(json);

        Assert.That(roundTrip, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(roundTrip!.TestId, Is.EqualTo(intent.TestId));
            Assert.That(roundTrip.SchemaVersion, Is.EqualTo(ContractSchemaVersions.Current));
            Assert.That(roundTrip.Title, Is.EqualTo(intent.Title));
            Assert.That(roundTrip.TestType, Is.EqualTo(intent.TestType));
            Assert.That(roundTrip.Actions.Count, Is.EqualTo(3));
            Assert.That(roundTrip.ExpectedResults.Count, Is.EqualTo(2));
            Assert.That(roundTrip.TestData["username"], Is.TypeOf<JsonElement>());
        });

        var username = ((JsonElement)roundTrip!.TestData["username"]!).GetString();
        Assert.That(username, Is.EqualTo("validUser"));
    }

    [Test]
    public void ExecutionContract_RoundTrip_PreservesExecutionKeywords()
    {
        var contract = new ExecutionContract
        {
            TestId = "TC-001",
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

        var json = AgenticJsonSerializer.Serialize(contract);
        Assert.That(json, Does.Contain("\"schemaVersion\": \"1.0\""));
        Assert.That(json, Does.Contain("\"keyword\": \"NAVIGATE\""));
        Assert.That(json, Does.Contain("\"keyword\": \"VERIFY_VISIBLE\""));

        var roundTrip = AgenticJsonSerializer.Deserialize<ExecutionContract>(json);

        Assert.That(roundTrip, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(roundTrip!.Steps.Count, Is.EqualTo(5));
            Assert.That(roundTrip.Steps[0].Keyword, Is.EqualTo(ExecutionKeyword.Navigate));
            Assert.That(roundTrip.Steps[4].Keyword, Is.EqualTo(ExecutionKeyword.VerifyVisible));
            Assert.That(roundTrip.Steps[1].Value, Is.EqualTo("{{username}}"));
        });
    }

    [Test]
    public void ElementRegistryAndResults_RoundTrip_PreserveLocatorAndStatuses()
    {
        var registry = new ElementRegistryModel
        {
            Elements =
            [
                new ElementRegistryEntry
                {
                    Target = "UsernameInput",
                    Page = "LoginPage",
                    LocatorType = LocatorType.GetByLabel,
                    LocatorValue = "Username"
                },
                new ElementRegistryEntry
                {
                    Target = "LoginButton",
                    Page = "LoginPage",
                    LocatorType = LocatorType.GetByRole,
                    LocatorValue = "button",
                    Name = "Login"
                }
            ]
        };

        var stepResult = new StepExecutionResult
        {
            TestId = "TC-001",
            Step = 4,
            Keyword = ExecutionKeyword.Click,
            Target = "LoginButton",
            Status = StepStatus.Blocked,
            StartedAt = DateTimeOffset.Parse("2026-09-23T15:10:00Z"),
            DurationMs = 120,
            Error = new ExecutionError
            {
                ErrorCode = LocatorValidationErrorCode.LocatorNotFound.ToString().ToUpperInvariant(),
                Message = "LoginButton could not be resolved."
            }
        };

        var result = new TestExecutionResult
        {
            TestId = "TC-001",
            Status = TestStatus.Blocked,
            TotalSteps = 5,
            PassedSteps = 2,
            FailedSteps = 0,
            BlockedSteps = 1,
            SkippedSteps = 2,
            StepResults = [stepResult]
        };

        var registryJson = AgenticJsonSerializer.Serialize(registry);
        var resultJson = AgenticJsonSerializer.Serialize(result);

        Assert.That(registryJson, Does.Contain("\"locatorType\": \"getByRole\""));
        Assert.That(resultJson, Does.Contain("\"status\": \"BLOCKED\""));

        var roundTripRegistry = AgenticJsonSerializer.Deserialize<ElementRegistryModel>(registryJson);
        var roundTripResult = AgenticJsonSerializer.Deserialize<TestExecutionResult>(resultJson);

        Assert.That(roundTripRegistry, Is.Not.Null);
        Assert.That(roundTripResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(roundTripRegistry!.Elements.Count, Is.EqualTo(2));
            Assert.That(roundTripRegistry.Elements[1].LocatorType, Is.EqualTo(LocatorType.GetByRole));
            Assert.That(roundTripResult!.Status, Is.EqualTo(TestStatus.Blocked));
            Assert.That(roundTripResult.StepResults[0].Status, Is.EqualTo(StepStatus.Blocked));
        });
    }
}
