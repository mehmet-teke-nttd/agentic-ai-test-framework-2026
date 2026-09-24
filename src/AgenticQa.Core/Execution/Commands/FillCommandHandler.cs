using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Commands;

public sealed class FillCommandHandler : TargetedCommandHandlerBase, IExecutionCommandHandler
{
    private readonly ILogger<FillCommandHandler> _logger;
    private readonly ITestDataResolver _testDataResolver;

    public FillCommandHandler(
        ILogger<FillCommandHandler> logger,
        IElementRegistryService elementRegistryService,
        ILocatorResolver locatorResolver,
        ILocatorValidator locatorValidator,
        ITestDataResolver testDataResolver)
        : base(elementRegistryService, locatorResolver, locatorValidator)
    {
        _logger = logger;
        _testDataResolver = testDataResolver;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.Fill;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: FILL {Target}", step.Step, step.Target);

        if (step.Value is null)
        {
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Fill step requires a value."));
        }

        var (locator, failureResult) = await ResolveValidatedLocatorAsync(step, context, cancellationToken);
        if (failureResult is not null)
        {
            return failureResult;
        }

        var resolved = _testDataResolver.Resolve(step.Value, context.TestIntent);
        if (!resolved.IsSuccess || resolved.ResolvedValue is null)
        {
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                resolved.ErrorCode ?? RuntimeExecutionErrorCode.TestDataReferenceNotFound,
                resolved.ErrorMessage ?? "Test data reference could not be resolved."));
        }

        try
        {
            await locator!.FillAsync(resolved.ResolvedValue);
            return PassedResult(step, context);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright fill action failed");
            return BlockedResult(step, context, ExecutionErrorFactory.FromRuntime(
                RuntimeExecutionErrorCode.PlaywrightActionFailed,
                "Playwright fill action failed."));
        }
    }
}
