using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ElementRegistryValidator : IElementRegistryValidator
{
    private readonly ILogger<ElementRegistryValidator> _logger;

    public ElementRegistryValidator(ILogger<ElementRegistryValidator> logger)
    {
        _logger = logger;
    }

    public ElementRegistryValidationResult Validate(Models.ElementRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = new List<ElementRegistryValidationError>();

        if (registry.Elements is null || registry.Elements.Count == 0)
        {
            errors.Add(new ElementRegistryValidationError
            {
                ErrorCode = ElementRegistryValidationErrorCode.EmptyRegistry,
                Message = "Element registry must contain at least one element."
            });
        }
        else
        {
            foreach (var entry in registry.Elements)
            {
                if (string.IsNullOrWhiteSpace(entry.Target))
                {
                    errors.Add(new ElementRegistryValidationError
                    {
                        ErrorCode = ElementRegistryValidationErrorCode.MissingTarget,
                        Message = "Element target is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(entry.Page))
                {
                    errors.Add(new ElementRegistryValidationError
                    {
                        ErrorCode = ElementRegistryValidationErrorCode.MissingPage,
                        Target = entry.Target,
                        Message = "Element page is required."
                    });
                }

                if (!Enum.IsDefined(entry.LocatorType))
                {
                    errors.Add(new ElementRegistryValidationError
                    {
                        ErrorCode = ElementRegistryValidationErrorCode.InvalidLocatorType,
                        Target = entry.Target,
                        Message = "Locator type is not supported."
                    });
                }

                if (string.IsNullOrWhiteSpace(entry.LocatorValue))
                {
                    errors.Add(new ElementRegistryValidationError
                    {
                        ErrorCode = ElementRegistryValidationErrorCode.MissingLocatorValue,
                        Target = entry.Target,
                        Message = "Locator value is required."
                    });
                }
            }

            var duplicateTargets = registry.Elements
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Target))
                .GroupBy(entry => entry.Target, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var duplicateTarget in duplicateTargets)
            {
                errors.Add(new ElementRegistryValidationError
                {
                    ErrorCode = ElementRegistryValidationErrorCode.DuplicateTarget,
                    Target = duplicateTarget,
                    Message = $"Duplicate target '{duplicateTarget}' is not allowed."
                });
            }
        }

        _logger.LogInformation(
            "Element registry validation completed. IsValid={IsValid}, ErrorCount={ErrorCount}",
            errors.Count == 0,
            errors.Count);

        return new ElementRegistryValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
