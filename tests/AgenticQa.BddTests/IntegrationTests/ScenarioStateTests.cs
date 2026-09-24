using AgenticQa.BddTests.Support;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.BddTests.IntegrationTests;

public class ScenarioStateTests
{
    [Test]
    public void QaScenarioContext_HoldsStronglyTypedState()
    {
        var state = new QaScenarioContext
        {
            TestIntent = LoginTestIntentFactory.Create(),
            ValidationResult = new TestIntentValidationResult
            {
                Status = ValidationStatus.Valid,
                Issues = []
            },
            ExecutionContract = new ExecutionContract
            {
                TestId = "TC-1",
                ExecutionType = "UI",
                Steps = []
            }
        };

        Assert.That(state.TestIntent, Is.Not.Null);
        Assert.That(state.ValidationResult?.Status, Is.EqualTo(ValidationStatus.Valid));
        Assert.That(state.ExecutionContract, Is.Not.Null);
    }
}
