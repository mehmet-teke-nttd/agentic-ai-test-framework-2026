using System.Text;
using System.Text.Json;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsBugDuplicateChecker : IBugDuplicateChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAzureDevOpsAuthenticationProvider _authProvider;
    private readonly AzureDevOpsOptions _options;

    public AzureDevOpsBugDuplicateChecker(
        IHttpClientFactory httpClientFactory,
        IAzureDevOpsAuthenticationProvider authProvider,
        IOptions<AzureDevOpsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _authProvider = authProvider;
        _options = options.Value;
    }

    public async Task<DuplicateBugMatch> FindDuplicateAsync(
        ApprovedBugDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (!_authProvider.TryCreateAuthorizationHeader(out var authHeader, out _))
        {
            return new DuplicateBugMatch
            {
                IsDuplicate = false,
                Reason = "Authentication not configured for duplicate check."
            };
        }

        var client = _httpClientFactory.CreateClient("AzureDevOps");
        var wiqlUrl = $"{_options.OrganizationUrl.TrimEnd('/')}/{_options.Project}/_apis/wit/wiql?api-version={_options.ApiVersion}";
        var wiql =
            $"{{\"query\":\"Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.TeamProject] = '{EscapeWiql(_options.Project)}' And [System.WorkItemType] = 'Bug' And [System.State] <> 'Closed' And [System.Title] = '{EscapeWiql(draft.Draft.Title)}' And [System.Tags] Contains '{EscapeWiql(draft.TestId)}'\"}}";

        using var request = new HttpRequestMessage(HttpMethod.Post, wiqlUrl);
        request.Headers.TryAddWithoutValidation("Authorization", authHeader);
        request.Content = new StringContent(wiql, Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new DuplicateBugMatch
            {
                IsDuplicate = false,
                Reason = "Duplicate check request failed."
            };
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("workItems", out var workItems)
            || workItems.ValueKind != JsonValueKind.Array
            || workItems.GetArrayLength() == 0)
        {
            return new DuplicateBugMatch { IsDuplicate = false };
        }

        var first = workItems.EnumerateArray().First();
        var id = first.TryGetProperty("id", out var idEl) ? idEl.ToString() : null;
        var url = first.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;

        return new DuplicateBugMatch
        {
            IsDuplicate = true,
            ExistingBugId = id,
            ExistingBugUrl = url,
            Reason = "Matching open bug found by title and test ID."
        };
    }

    private static string EscapeWiql(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);
}
