using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Interfaces;

public interface IAzureDevOpsPriorityMapper
{
    bool TryMap(BugPriority priority, out int mappedPriority, out string errorMessage);
}
