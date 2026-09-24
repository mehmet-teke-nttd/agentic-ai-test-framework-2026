using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IClarificationService
{
    ClarificationWorkflowResult ApplyClarification(
        TestIntent testIntent,
        TestIntentValidationResult currentValidation,
        ClarificationResponse clarificationResponse);
}
