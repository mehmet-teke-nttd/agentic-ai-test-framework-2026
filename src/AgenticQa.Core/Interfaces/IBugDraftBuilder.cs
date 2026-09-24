using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugDraftBuilder
{
    BugDraft Build(
        TestIntent testIntent,
        ExecutionContract executionContract,
        TestExecutionResult testExecutionResult,
        FailureAnalysisResult failureAnalysisResult,
        IReadOnlyList<string>? artifactReferences = null);
}
