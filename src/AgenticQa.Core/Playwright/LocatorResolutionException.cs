using AgenticQa.Core.Enums;

namespace AgenticQa.Core.Playwright;

public sealed class LocatorResolutionException : Exception
{
    public LocatorResolutionException(LocatorValidationErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public LocatorValidationErrorCode ErrorCode { get; }
}
