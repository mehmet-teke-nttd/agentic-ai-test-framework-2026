using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Validation;

public sealed class NoOpAmbiguityDetector : IAmbiguityDetector
{
    public IReadOnlyList<ClarificationIssue> Detect(TestIntent testIntent)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        return [];
    }
}
