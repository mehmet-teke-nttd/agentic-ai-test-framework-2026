using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Commands;

public sealed class VerifyVisibleCommandHandler : TargetedCommandHandlerBase, IExecutionCommandHandler
{
    private readonly ILogger<VerifyVisibleCommandHandler> _logger;
    private readonly IPlaywrightAssertionService _assertionService;

    public VerifyVisibleCommandHandler(
        ILogger<VerifyVisibleCommandHandler> logger,
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator,
        IPlaywrightAssertionService assertionService)
        : base(elementRegistryService, locatorResolver, locatorValidator)
    {
        _logger = logger;
        _assertionService = assertionService;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.VerifyVisible;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: VERIFY_VISIBLE {Target}", step.Step, step.Target);

        var (locator, failureResult) = await ResolveValidatedLocatorAsync(step, context, cancellationToken);
        if (failureResult is not null)
        {
            return failureResult;
        }

        try
        {
            await _assertionService.AssertVisibleAsync(locator!);
            return PassedResult(step, context);
        }
        catch (AssertionFailedException ex)
        {
            _logger.LogWarning(ex, "Visible assertion failed");
            return FailedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.AssertionFailed,
                "Expected element was not visible."));
        }
    }
}
