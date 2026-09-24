namespace AgenticQa.Core.Models;

public sealed class ApiEndpointRegistry
{
    public List<ApiEndpointEntry> Endpoints { get; set; } = [];
}

public sealed class ApiEndpointEntry
{
    public string Name { get; set; } = string.Empty;
    public string BasePath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string AuthProfile { get; set; } = string.Empty;
    public string SchemaAssertion { get; set; } = string.Empty;
}
