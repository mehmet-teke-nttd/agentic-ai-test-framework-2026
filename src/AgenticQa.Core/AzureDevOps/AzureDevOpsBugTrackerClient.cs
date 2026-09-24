using System.Text;
using System.Text.Json;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsBugTrackerClient : IBugTrackerClient
{
    private readonly ILogger<AzureDevOpsBugTrackerClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAzureDevOpsAuthenticationProvider _authProvider;
    private readonly IAzureDevOpsBugRequestBuilder _requestBuilder;
    private readonly AzureDevOpsOptions _options;

    public AzureDevOpsBugTrackerClient(
        ILogger<AzureDevOpsBugTrackerClient> logger,
        IHttpClientFactory httpClientFactory,
        IAzureDevOpsAuthenticationProvider authProvider,
        IAzureDevOpsBugRequestBuilder requestBuilder,
        IOptions<AzureDevOpsOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _authProvider = authProvider;
        _requestBuilder = requestBuilder;
        _options = options.Value;
    }

    public async Task<BugCreationResult> CreateBugAsync(
        ApprovedBugDraft draft,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bug creation requested for TestId={TestId}", draft.TestId);

        if (!_requestBuilder.TryBuild(draft, out var request, out var mappingError))
        {
            return Failed(draft.TestId, BugCreationErrorCode.AzureDevOpsFieldMappingFailed, mappingError.Message, draft.CreationKey);
        }

        if (_options.DryRun)
        {
            return new BugCreationResult
            {
                TestId = draft.TestId,
                CreationKey = draft.CreationKey,
                Status = BugCreationStatus.ReadyForCreation,
                Message = "Dry run enabled. Request validated but not submitted.",
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        if (!_authProvider.TryCreateAuthorizationHeader(out var authHeader, out var authError))
        {
            return Failed(draft.TestId, BugCreationErrorCode.AzureDevOpsAuthenticationFailed, authError, draft.CreationKey);
        }

        var client = _httpClientFactory.CreateClient("AzureDevOps");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.Url);
        httpRequest.Headers.TryAddWithoutValidation("Authorization", authHeader);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request.Operations),
            Encoding.UTF8,
            "application/json-patch+json");

        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Failed(
                draft.TestId,
                BugCreationErrorCode.AzureDevOpsRequestFailed,
                $"Azure DevOps request failed with status {(int)response.StatusCode}.",
                draft.CreationKey);
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var id = root.TryGetProperty("id", out var idEl) ? idEl.ToString() : null;
            var url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return Failed(
                    draft.TestId,
                    BugCreationErrorCode.AzureDevOpsResponseInvalid,
                    "Azure DevOps response did not contain a work item id.",
                    draft.CreationKey);
            }

            _logger.LogInformation("Azure DevOps bug created for TestId={TestId}", draft.TestId);
            return new BugCreationResult
            {
                TestId = draft.TestId,
                CreationKey = draft.CreationKey,
                Status = BugCreationStatus.Created,
                ExternalId = id,
                ExternalUrl = url,
                CreatedAt = DateTimeOffset.UtcNow,
                Message = "Bug created successfully."
            };
        }
        catch (JsonException)
        {
            return Failed(
                draft.TestId,
                BugCreationErrorCode.AzureDevOpsResponseInvalid,
                "Azure DevOps response could not be parsed.",
                draft.CreationKey);
        }
    }

    private static BugCreationResult Failed(
        string testId,
        BugCreationErrorCode errorCode,
        string message,
        string? creationKey) =>
        new()
        {
            TestId = testId,
            CreationKey = creationKey,
            Status = BugCreationStatus.Failed,
            Message = message,
            Error = new ExecutionError
            {
                ErrorCode = errorCode switch
                {
                    BugCreationErrorCode.AzureDevOpsConfigurationInvalid => "AZURE_DEVOPS_CONFIGURATION_INVALID",
                    BugCreationErrorCode.AzureDevOpsAuthenticationFailed => "AZURE_DEVOPS_AUTHENTICATION_FAILED",
                    BugCreationErrorCode.AzureDevOpsRequestFailed => "AZURE_DEVOPS_REQUEST_FAILED",
                    BugCreationErrorCode.AzureDevOpsFieldMappingFailed => "AZURE_DEVOPS_FIELD_MAPPING_FAILED",
                    BugCreationErrorCode.AzureDevOpsResponseInvalid => "AZURE_DEVOPS_RESPONSE_INVALID",
                    _ => "AZURE_DEVOPS_REQUEST_FAILED"
                },
                Message = message
            }
        };
}
