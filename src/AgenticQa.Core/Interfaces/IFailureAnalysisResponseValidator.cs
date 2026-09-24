using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IFailureAnalysisResponseValidator
{
    bool TryParse(
        string testId,
        string aiResponse,
        out FailureAnalysisResult result,
        out string validationError);
}
