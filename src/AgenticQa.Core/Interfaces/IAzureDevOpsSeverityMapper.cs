using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Interfaces;

public interface IAzureDevOpsSeverityMapper
{
    bool TryMap(BugSeverity severity, out string mappedSeverity, out string errorMessage);
}
