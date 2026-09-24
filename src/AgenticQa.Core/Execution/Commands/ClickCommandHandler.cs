using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Commands;

public sealed class ClickCommandHandler : TargetedCommandHandlerBase, IExecutionCommandHandler
{
    private readonly ILogger<ClickCommandHandler> _logger;

    public ClickCommandHandler(
        ILogger<ClickCommandHandler> logger,
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator)
        : base(elementRegistryService, locatorResolver, locatorValidator)
    {
        _logger = logger;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.Click;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: CLICK {Target}", step.Step, step.Target);

        var (locator, failureResult) = await ResolveValidatedLocatorAsync(step, context, cancellationToken);
        if (failureResult is not null)
        {
            return failureResult;
        }

        try
        {
            await locator!.ClickAsync();
            return PassedResult(step, context);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright click action failed");
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Playwright click action failed."));
        }
    }
}
