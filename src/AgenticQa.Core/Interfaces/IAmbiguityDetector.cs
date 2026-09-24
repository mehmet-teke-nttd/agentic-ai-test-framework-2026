using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IAmbiguityDetector
{
    IReadOnlyList<ClarificationIssue> Detect(TestIntent testIntent);
}
