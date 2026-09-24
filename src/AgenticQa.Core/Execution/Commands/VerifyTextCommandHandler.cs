using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Commands;

public sealed class VerifyTextCommandHandler : TargetedCommandHandlerBase, IExecutionCommandHandler
{
    private readonly ILogger<VerifyTextCommandHandler> _logger;
    private readonly IPlaywrightAssertionService _assertionService;
    private readonly ITestDataResolver _testDataResolver;

    public VerifyTextCommandHandler(
        ILogger<VerifyTextCommandHandler> logger,
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator,
        IPlaywrightAssertionService assertionService,
        ITestDataResolver testDataResolver)
        : base(elementRegistryService, locatorResolver, locatorValidator)
    {
        _logger = logger;
        _assertionService = assertionService;
        _testDataResolver = testDataResolver;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.VerifyText;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: VERIFY_TEXT {Target}", step.Step, step.Target);

        if (string.IsNullOrWhiteSpace(step.Expected))
        {
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Verify text step requires an expected value."));
        }

        var (locator, failureResult) = await ResolveValidatedLocatorAsync(step, context, cancellationToken);
        if (failureResult is not null)
        {
            return failureResult;
        }

        var resolvedExpected = _testDataResolver.Resolve(step.Expected, context.TestIntent);
        if (!resolvedExpected.IsSuccess || resolvedExpected.ResolvedValue is null)
        {
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                resolvedExpected.ErrorCode ?? RuntimeExecutionErrorCode.TestDataReferenceNotFound,
                resolvedExpected.ErrorMessage ?? "Expected text reference could not be resolved."));
        }

        try
        {
            await _assertionService.AssertTextAsync(locator!, resolvedExpected.ResolvedValue);
            return PassedResult(step, context);
        }
        catch (AssertionFailedException ex)
        {
            _logger.LogWarning(ex, "Text assertion failed");
            return FailedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.AssertionFailed,
                "Expected text did not match."));
        }
    }
}
