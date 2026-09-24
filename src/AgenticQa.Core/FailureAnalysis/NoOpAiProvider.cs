using AgenticQa.Core.Interfaces;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class NoOpAiProvider : IAiProvider
{
    public Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        const string response =
            """
            {
              "classification": "UNKNOWN",
              "confidence": 0.0,
              "summary": "AI provider is not configured.",
              "evidence": [
                "No AI provider implementation was configured for this environment."
              ],
              "recommendedAction": "QA_REVIEW_REQUIRED",
              "escalationRequired": true,
              "escalationLevel": "MEDIUM",
              "escalationReason": "AI provider is not configured for this environment."
            }
            """;

        return Task.FromResult(response);
    }
}
