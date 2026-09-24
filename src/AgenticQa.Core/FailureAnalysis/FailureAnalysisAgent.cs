using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.FailureAnalysis;

public sealed class FailureAnalysisAgent : IFailureAnalysisAgent
{
    private readonly ILogger<FailureAnalysisAgent> _logger;
    private readonly IAiProvider _aiProvider;
    private readonly IFailureAnalysisPromptBuilder _promptBuilder;
    private readonly IFailureAnalysisResponseValidator _responseValidator;

    public FailureAnalysisAgent(
        ILogger<FailureAnalysisAgent> logger,
        IAiProvider aiProvider,
        IFailureAnalysisPromptBuilder promptBuilder,
        IFailureAnalysisResponseValidator responseValidator)
    {
        _logger = logger;
        _aiProvider = aiProvider;
        _promptBuilder = promptBuilder;
        _responseValidator = responseValidator;
    }

    public async Task<FailureAnalysisResult> AnalyzeAsync(
        FailureAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Failure analysis started for TestId={TestId}", request.TestId);
        var prompt = _promptBuilder.Build(request);

        string response;
        try
        {
            _logger.LogInformation("AI analysis requested for TestId={TestId}", request.TestId);
            response = await _aiProvider.GenerateAsync(
                prompt.SystemPrompt,
                prompt.UserPrompt,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider call failed for TestId={TestId}", request.TestId);
            return FailureAnalysisResponseValidator.UnknownResult(
                request.TestId,
                "AI provider call failed.");
        }

        if (_responseValidator.TryParse(request.TestId, response, out var result, out var error))
        {
            _logger.LogInformation("AI response validated for TestId={TestId}", request.TestId);
            return result;
        }

        _logger.LogWarning(
            "AI response validation failed for TestId={TestId}: {Error}",
            request.TestId,
            error);
        return FailureAnalysisResponseValidator.UnknownResult(
            request.TestId,
            "AI response was invalid.");
    }
}
