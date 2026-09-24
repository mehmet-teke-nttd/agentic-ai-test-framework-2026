using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface IBugDraftResponseValidator
{
    bool TryParse(string aiResponse, out BugDraftPolishResult result, out string validationError);
}
