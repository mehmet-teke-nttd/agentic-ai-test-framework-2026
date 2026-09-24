using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Commands;

public sealed class CheckCommandHandler : TargetedCommandHandlerBase, IExecutionCommandHandler
{
    private readonly ILogger<CheckCommandHandler> _logger;

    public CheckCommandHandler(
        ILogger<CheckCommandHandler> logger,
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator)
        : base(elementRegistryService, locatorResolver, locatorValidator)
    {
        _logger = logger;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.Check;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: CHECK {Target}", step.Step, step.Target);

        var (locator, failureResult) = await ResolveValidatedLocatorAsync(step, context, cancellationToken);
        if (failureResult is not null)
        {
            return failureResult;
        }

        try
        {
            await locator!.CheckAsync();
            return PassedResult(step, context);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright check action failed");
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Playwright check action failed."));
        }
    }
}
