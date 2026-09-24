using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestIntentValidator
{
    TestIntentValidationResult Validate(TestIntent testIntent);
}
