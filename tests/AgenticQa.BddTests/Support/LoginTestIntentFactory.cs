using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.BddTests.Support;

public static class LoginTestIntentFactory
{
    public static TestIntent Create()
    {
        var username = Environment.GetEnvironmentVariable("QA_USERNAME") ?? "testuser";
        var password = Environment.GetEnvironmentVariable("QA_PASSWORD") ?? "testpassword";

        return new TestIntent
        {
            TestId = "TC-LOGIN-001",
            Title = "Successful login with valid credentials",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User account exists"],
            TestData = new Dictionary<string, object?>
            {
                ["username"] = username,
                ["password"] = password
            },
            Actions =
            [
                new TestAction { Step = 1, Action = "Navigate to Login page" },
                new TestAction { Step = 2, Action = "Enter username" },
                new TestAction { Step = 3, Action = "Enter password" },
                new TestAction { Step = 4, Action = "Click Login button" },
                new TestAction { Step = 5, Action = "Verify Dashboard is visible" }
            ],
            ExpectedResults = ["Dashboard page is displayed"]
        };
    }
}
