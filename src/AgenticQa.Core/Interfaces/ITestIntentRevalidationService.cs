using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestIntentRevalidationService
{
    ClarificationWorkflowResult Revalidate(
        TestIntent testIntent,
        TestIntentValidationResult currentValidation,
        ClarificationResponse clarificationResponse);
}
