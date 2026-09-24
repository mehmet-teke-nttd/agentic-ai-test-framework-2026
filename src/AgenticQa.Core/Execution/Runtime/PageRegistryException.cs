using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class PageRegistryException : Exception
{
    public PageRegistryException(RuntimeExecutionErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public RuntimeExecutionErrorCode ErrorCode { get; }
}
