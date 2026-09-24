using AgenticQa.Core.Enums;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ElementLookupException : Exception
{
    public ElementLookupException(LocatorValidationErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public LocatorValidationErrorCode ErrorCode { get; }
}
