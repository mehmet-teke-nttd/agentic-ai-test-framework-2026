namespace AgenticQa.Core.Models;

public sealed class AzureDevOpsOptions
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "7.1";
    public string PersonalAccessTokenEnvironmentVariable { get; set; } = "AZURE_DEVOPS_PAT";
    public bool DryRun { get; set; } = true;
    public Dictionary<string, string> SeverityMapping { get; set; } = [];
    public Dictionary<string, int> PriorityMapping { get; set; } = [];
    public int? DefaultPriority { get; set; }
}

public sealed class AzureDevOpsJsonPatchOperation
{
    public string Op { get; init; } = "add";
    public string Path { get; init; } = string.Empty;
    public object? Value { get; init; }
}

public sealed class AzureDevOpsBugRequest
{
    public string Url { get; init; } = string.Empty;
    public IReadOnlyList<AzureDevOpsJsonPatchOperation> Operations { get; init; } = [];
}

public sealed class DuplicateBugMatch
{
    public bool IsDuplicate { get; init; }
    public string? ExistingBugId { get; init; }
    public string? ExistingBugUrl { get; init; }
    public string? Reason { get; init; }
}
