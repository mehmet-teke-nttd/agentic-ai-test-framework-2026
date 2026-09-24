namespace AgenticQa.Core.Models;

public sealed class PageRegistryConfiguration
{
    public List<PageRegistryEntry> Pages { get; set; } = [];
}

public sealed class PageRegistryEntry
{
    public string Page { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
