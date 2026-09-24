using System.Text;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsAuthenticationProvider : IAzureDevOpsAuthenticationProvider
{
    private readonly AzureDevOpsOptions _options;

    public AzureDevOpsAuthenticationProvider(IOptions<AzureDevOpsOptions> options)
    {
        _options = options.Value;
    }

    public bool TryCreateAuthorizationHeader(out string authorizationHeader, out string errorMessage)
    {
        var envName = string.IsNullOrWhiteSpace(_options.PersonalAccessTokenEnvironmentVariable)
            ? "AZURE_DEVOPS_PAT"
            : _options.PersonalAccessTokenEnvironmentVariable;
        var pat = Environment.GetEnvironmentVariable(envName);

        if (string.IsNullOrWhiteSpace(pat))
        {
            authorizationHeader = string.Empty;
            errorMessage = $"Azure DevOps PAT environment variable '{envName}' is missing.";
            return false;
        }

        var raw = $":{pat}";
        var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(raw));
        authorizationHeader = $"Basic {base64}";
        errorMessage = string.Empty;
        return true;
    }
}
