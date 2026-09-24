using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IAzureDevOpsBugRequestBuilder
{
    bool TryBuild(
        ApprovedBugDraft approvedDraft,
        out AzureDevOpsBugRequest request,
        out ExecutionError error);
}
