using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsPriorityMapper : IAzureDevOpsPriorityMapper
{
    private readonly AzureDevOpsOptions _options;

    public AzureDevOpsPriorityMapper(IOptions<AzureDevOpsOptions> options)
    {
        _options = options.Value;
    }

    public bool TryMap(BugPriority priority, out int mappedPriority, out string errorMessage)
    {
        if (priority == BugPriority.Unassigned)
        {
            if (_options.DefaultPriority.HasValue)
            {
                mappedPriority = _options.DefaultPriority.Value;
                errorMessage = string.Empty;
                return true;
            }

            mappedPriority = default;
            errorMessage = "Priority is UNASSIGNED and no default priority is configured.";
            return false;
        }

        var key = priority switch
        {
            BugPriority.Low => "LOW",
            BugPriority.Medium => "MEDIUM",
            BugPriority.High => "HIGH",
            _ => "LOW"
        };

        if (_options.PriorityMapping.TryGetValue(key, out mappedPriority))
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = $"Priority mapping for '{key}' is not configured.";
        return false;
    }
}
