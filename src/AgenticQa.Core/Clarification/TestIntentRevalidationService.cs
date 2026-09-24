using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Clarification;

public sealed class TestIntentRevalidationService : ITestIntentRevalidationService
{
    private readonly ILogger<TestIntentRevalidationService> _logger;
    private readonly IClarificationService _clarificationService;

    public TestIntentRevalidationService(
        ILogger<TestIntentRevalidationService> logger,
        IClarificationService clarificationService)
    {
        _logger = logger;
        _clarificationService = clarificationService;
    }

    public ClarificationWorkflowResult Revalidate(
        TestIntent testIntent,
        TestIntentValidationResult currentValidation,
        ClarificationResponse clarificationResponse)
    {
        var result = _clarificationService.ApplyClarification(
            testIntent,
            currentValidation,
            clarificationResponse);

        _logger.LogInformation(
            "Revalidation completed for TestId={TestId}. Accepted={Accepted}, Blocked={Blocked}, Status={Status}",
            testIntent.TestId,
            result.IsAccepted,
            result.IsBlocked,
            result.ValidationResult.Status);

        return result;
    }
}
