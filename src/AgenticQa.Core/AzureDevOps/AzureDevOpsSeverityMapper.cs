using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsSeverityMapper : IAzureDevOpsSeverityMapper
{
    private readonly AzureDevOpsOptions _options;

    public AzureDevOpsSeverityMapper(IOptions<AzureDevOpsOptions> options)
    {
        _options = options.Value;
    }

    public bool TryMap(BugSeverity severity, out string mappedSeverity, out string errorMessage)
    {
        var key = severity switch
        {
            BugSeverity.Low => "LOW",
            BugSeverity.Medium => "MEDIUM",
            BugSeverity.High => "HIGH",
            BugSeverity.Critical => "CRITICAL",
            _ => "MEDIUM"
        };

        if (_options.SeverityMapping.TryGetValue(key, out mappedSeverity!))
        {
            errorMessage = string.Empty;
            return true;
        }

        mappedSeverity = string.Empty;
        errorMessage = $"Severity mapping for '{key}' is not configured.";
        return false;
    }
}
