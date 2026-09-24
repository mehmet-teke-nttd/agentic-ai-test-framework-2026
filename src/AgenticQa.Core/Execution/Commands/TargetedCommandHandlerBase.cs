using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Playwright;
using AgenticQa.Core.Execution.Runtime;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Commands;

public abstract class TargetedCommandHandlerBase
{
    private readonly IElementRegistryService _elementRegistryService;
    private readonly ILocatorResolver _locatorResolver;
    private readonly ILocatorValidator _locatorValidator;

    protected TargetedCommandHandlerBase(
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator)
    {
        _elementRegistryService = elementRegistryService;
        _locatorResolver = locatorResolver;
        _locatorValidator = locatorValidator;
    }

    protected async Task<(ILocator? Locator, StepExecutionResult? FailureResult)> ResolveValidatedLocatorAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(step.Target))
        {
            return (null, BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Step target is required.")));
        }

        ElementRegistryEntry element;
        try
        {
            element = _elementRegistryService.GetElement(step.Target);
        }
        catch (ElementLookupException ex)
        {
            return (null, BlockedResult(step, context, ExecutionErrorFactory.FromLocator(ex.ErrorCode, ex.Message)));
        }

        ILocator locator;
        try
        {
            locator = _locatorResolver.Resolve(context.Page, element);
        }
        catch (LocatorResolutionException ex)
        {
            return (null, BlockedResult(step, context, ExecutionErrorFactory.FromLocator(ex.ErrorCode, ex.Message)));
        }
        catch (InvalidOperationException ex)
        {
            return (null, BlockedResult(step, context, ExecutionErrorFactory.FromLocator(
                LocatorValidationErrorCode.InvalidLocatorConfig,
                ex.Message)));
        }

        var expectedPage = context.CurrentPage ?? element.Page;
        var validation = await _locatorValidator.ValidateAsync(locator, element, expectedPage);
        if (validation.Status != LocatorValidationStatus.Valid)
        {
            return (null, BlockedResult(
                step,
                context,
                validation.Error ?? ExecutionErrorFactory.FromRuntime(
                    RuntimeExecutionErrorCode.PlaywrightActionFailed,
                    "Locator validation failed.")));
        }

        return (locator, null);
    }

    protected static StepExecutionResult PassedResult(ExecutionStep step, ExecutionContext context) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Passed,
            Error = null
        };

    protected static StepExecutionResult FailedResult(
        ExecutionStep step,
        ExecutionContext context,
        ExecutionError error) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Failed,
            Error = error
        };

    protected static StepExecutionResult BlockedResult(
        ExecutionStep step,
        ExecutionContext context,
        ExecutionError error) =>
        new()
        {
            TestId = context.TestIntent.TestId,
            Step = step.Step,
            Keyword = step.Keyword,
            Target = step.Target,
            Status = StepStatus.Blocked,
            Error = error
        };
}
