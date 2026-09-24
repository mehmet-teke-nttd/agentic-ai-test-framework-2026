namespace AgenticQa.Core.Execution.Runtime;

public sealed class AssertionFailedException : Exception
{
    public AssertionFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
