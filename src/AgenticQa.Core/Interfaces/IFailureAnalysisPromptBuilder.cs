using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IFailureAnalysisPromptBuilder
{
    FailureAnalysisPrompt Build(FailureAnalysisRequest request);
}
