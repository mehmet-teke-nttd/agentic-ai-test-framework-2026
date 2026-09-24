namespace AgenticQa.Core.Interfaces;

public interface IAzureDevOpsAuthenticationProvider
{
    bool TryCreateAuthorizationHeader(out string authorizationHeader, out string errorMessage);
}
