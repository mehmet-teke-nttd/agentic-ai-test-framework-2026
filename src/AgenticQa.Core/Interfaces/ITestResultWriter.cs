using AgenticQa.Core.Models;

namespace AgenticQa.Core.Interfaces;

public interface ITestResultWriter
{
    Task<string> WriteAsync(TestExecutionResult result, CancellationToken cancellationToken = default);
}
