using Microsoft.Playwright;

namespace AgenticQa.Core.Models;

public sealed class ExecutionContext
{
    public required IPage Page { get; init; }
    public required TestIntent TestIntent { get; init; }
    public string? CurrentPage { get; set; }
}
