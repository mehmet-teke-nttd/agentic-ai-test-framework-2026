using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Playwright;

public sealed class PlaywrightLocatorValidator : ILocatorValidator
{
    private readonly ILogger<PlaywrightLocatorValidator> _logger;

    public PlaywrightLocatorValidator(ILogger<PlaywrightLocatorValidator> logger)
    {
        _logger = logger;
    }

    public async Task<LocatorValidationResult> ValidateAsync(
        ILocator locator,
        ElementRegistryEntry element,
        string expectedPage)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(element);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedPage);

        _logger.LogInformation(
            "Locator validation started for target {Target}, expected page {ExpectedPage}",
            element.Target,
            expectedPage);

        if (!string.Equals(element.Page, expectedPage, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                element,
                expectedPage,
                null,
                LocatorValidationErrorCode.PageMismatch,
                $"Element page '{element.Page}' does not match expected page '{expectedPage}'.");
        }

        var count = await locator.CountAsync();
        if (count == 0)
        {
            return Failure(
                element,
                expectedPage,
                count,
                LocatorValidationErrorCode.LocatorNotFound,
                "No element matched the configured locator.");
        }

        if (count > 1)
        {
            return Failure(
                element,
                expectedPage,
                count,
                LocatorValidationErrorCode.LocatorNotUnique,
                "Locator matched more than one element.");
        }

        return new LocatorValidationResult
        {
            Status = LocatorValidationStatus.Valid,
            Target = element.Target,
            ExpectedPage = expectedPage,
            MatchCount = count,
            Error = null
        };
    }

    private LocatorValidationResult Failure(
        ElementRegistryEntry element,
        string expectedPage,
        int? count,
        LocatorValidationErrorCode errorCode,
        string message)
    {
        _logger.LogWarning(
            "Locator validation failed for target {Target} with code {ErrorCode}",
            element.Target,
            errorCode);

        return new LocatorValidationResult
        {
            Status = LocatorValidationStatus.Invalid,
            Target = element.Target,
            ExpectedPage = expectedPage,
            MatchCount = count,
            Error = new ExecutionError
            {
                ErrorCode = ToContractErrorCode(errorCode),
                Message = message
            }
        };
    }

    private static string ToContractErrorCode(LocatorValidationErrorCode errorCode) =>
        errorCode switch
        {
            LocatorValidationErrorCode.TargetNotRegistered => "TARGET_NOT_REGISTERED",
            LocatorValidationErrorCode.PageMismatch => "PAGE_MISMATCH",
            LocatorValidationErrorCode.LocatorNotFound => "LOCATOR_NOT_FOUND",
            LocatorValidationErrorCode.LocatorNotUnique => "LOCATOR_NOT_UNIQUE",
            LocatorValidationErrorCode.InvalidLocatorConfig => "INVALID_LOCATOR_CONFIG",
            _ => errorCode.ToString().ToUpperInvariant()
        };
}
