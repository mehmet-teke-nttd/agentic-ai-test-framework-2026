using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Models;

public sealed class ComponentRegistry
{
    public List<ComponentRegistryEntry> Components { get; set; } = [];
}

public sealed class ComponentRegistryEntry
{
    public string Name { get; set; } = string.Empty;
    public List<string> Pages { get; set; } = [];
    public List<ComponentElementEntry> Elements { get; set; } = [];
}

public sealed class ComponentElementEntry
{
    public string Target { get; set; } = string.Empty;
    public LocatorType LocatorType { get; set; }
    public string LocatorValue { get; set; } = string.Empty;
    public string? Name { get; set; }
}
