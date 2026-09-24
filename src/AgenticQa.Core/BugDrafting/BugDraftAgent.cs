using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.BugDrafting;

public sealed class BugDraftAgent : IBugDraftAgent
{
    private readonly ILogger<BugDraftAgent> _logger;
    private readonly IAiProvider _aiProvider;
    private readonly IBugDraftResponseValidator _responseValidator;

    public BugDraftAgent(
        ILogger<BugDraftAgent> logger,
        IAiProvider aiProvider,
        IBugDraftResponseValidator responseValidator)
    {
        _logger = logger;
        _aiProvider = aiProvider;
        _responseValidator = responseValidator;
    }

    public async Task<BugDraftPolishResult?> PolishAsync(
        BugDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var systemPrompt =
            """
            You improve wording for a QA bug draft.
            Use only supplied evidence.
            Do not invent facts.
            Do not invent reproduction steps.
            Do not assign priority.
            Keep title concise.
            Return strict JSON with keys: title, summary, actualResult.
            """;

        var userPrompt =
            $"""
             testId: {request.TestId}
             title: {request.Title}
             actions:
             {string.Join(Environment.NewLine, request.Actions.Select(a => $"- {a}"))}
             expectedResults:
             {string.Join(Environment.NewLine, request.ExpectedResults.Select(e => $"- {e}"))}
             failedStep: {request.FailedStep}
             actualError: {request.ActualError ?? "N/A"}
             failureClassification: {request.FailureClassification}
             qaReview: {request.FailureReviewDecision}
             environment: {request.EnvironmentMetadata}
             evidence:
             {string.Join(Environment.NewLine, request.EvidenceReferences.Select(e => $"- {e}"))}
             """;

        string response;
        try
        {
            response = await _aiProvider.GenerateAsync(systemPrompt, userPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bug draft AI polishing failed for TestId={TestId}", request.TestId);
            return null;
        }

        if (_responseValidator.TryParse(response, out var polished, out var error))
        {
            _logger.LogInformation("Bug draft polished for TestId={TestId}", request.TestId);
            return polished;
        }

        _logger.LogWarning(
            "Bug draft AI response validation failed for TestId={TestId}: {Error}",
            request.TestId,
            error);
        return null;
    }
}
