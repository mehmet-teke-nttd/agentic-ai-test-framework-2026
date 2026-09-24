using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestDataResolver
{
    TestDataResolutionResult Resolve(string value, TestIntent testIntent);
}
