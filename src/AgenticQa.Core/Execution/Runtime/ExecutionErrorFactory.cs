using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.Execution.Runtime;

internal static class ExecutionErrorFactory
{
    public static ExecutionError FromRuntime(RuntimeExecutionErrorCode errorCode, string message) =>
        new()
        {
            ErrorCode = ToRuntimeErrorCodeString(errorCode),
            Message = message
        };

    public static ExecutionError FromLocator(LocatorValidationErrorCode errorCode, string message) =>
        new()
        {
            ErrorCode = errorCode switch
            {
                LocatorValidationErrorCode.TargetNotRegistered => "TARGET_NOT_REGISTERED",
                LocatorValidationErrorCode.PageMismatch => "PAGE_MISMATCH",
                LocatorValidationErrorCode.LocatorNotFound => "LOCATOR_NOT_FOUND",
                LocatorValidationErrorCode.LocatorNotUnique => "LOCATOR_NOT_UNIQUE",
                LocatorValidationErrorCode.InvalidLocatorConfig => "INVALID_LOCATOR_CONFIG",
                _ => errorCode.ToString().ToUpperInvariant()
            },
            Message = message
        };

    public static ExecutionError FromTest(TestExecutionErrorCode errorCode, string message) =>
        new()
        {
            ErrorCode = errorCode switch
            {
                TestExecutionErrorCode.ExecutionContractInvalid => "EXECUTION_CONTRACT_INVALID",
                TestExecutionErrorCode.TestIntentInvalid => "TEST_INTENT_INVALID",
                TestExecutionErrorCode.ExecutionConfigurationError => "EXECUTION_CONFIGURATION_ERROR",
                TestExecutionErrorCode.ExecutionCancelled => "EXECUTION_CANCELLED",
                _ => errorCode.ToString().ToUpperInvariant()
            },
            Message = message
        };

    private static string ToRuntimeErrorCodeString(RuntimeExecutionErrorCode errorCode) =>
        errorCode switch
        {
            RuntimeExecutionErrorCode.AssertionFailed => "ASSERTION_FAILED",
            RuntimeExecutionErrorCode.PageNotRegistered => "PAGE_NOT_REGISTERED",
            RuntimeExecutionErrorCode.NavigationFailed => "NAVIGATION_FAILED",
            RuntimeExecutionErrorCode.PlaywrightActionFailed => "PLAYWRIGHT_ACTION_FAILED",
            RuntimeExecutionErrorCode.TestDataReferenceNotFound => "TEST_DATA_REFERENCE_NOT_FOUND",
            RuntimeExecutionErrorCode.UnsupportedHandler => "UNSUPPORTED_HANDLER",
            _ => errorCode.ToString().ToUpperInvariant()
        };
}
