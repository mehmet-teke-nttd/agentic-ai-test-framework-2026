using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Commands;

public sealed class NavigateCommandHandler : IExecutionCommandHandler
{
    private readonly ILogger<NavigateCommandHandler> _logger;
    private readonly IPageRegistry _pageRegistry;

    public NavigateCommandHandler(
        ILogger<NavigateCommandHandler> logger,
        IPageRegistry pageRegistry)
    {
        _logger = logger;
        _pageRegistry = pageRegistry;
    }

    public ExecutionKeyword Keyword => ExecutionKeyword.Navigate;

    public async Task<StepExecutionResult> ExecuteAsync(
        ExecutionStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing step {Step}: NAVIGATE {Target}", step.Step, step.Target);

        if (string.IsNullOrWhiteSpace(step.Target))
        {
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Blocked,
                Error = ExecutionErrorFactory.FromRuntime(
                    RuntimeExecutionErrorCode.NavigationFailed,
                    "Navigate step requires a target page.")
            };
        }

        string url;
        try
        {
            url = _pageRegistry.GetUrl(step.Target);
        }
        catch (PageRegistryException ex)
        {
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Blocked,
                Error = ExecutionErrorFactory.FromRuntime(ex.ErrorCode, ex.Message)
            };
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await context.Page.GotoAsync(url);
            context.CurrentPage = step.Target;
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Passed,
                Error = null
            };
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Navigation failed for page {Target}", step.Target);
            return new StepExecutionResult
            {
                TestId = context.TestIntent.TestId,
                Step = step.Step,
                Keyword = step.Keyword,
                Target = step.Target,
                Status = StepStatus.Blocked,
                Error = ExecutionErrorFactory.FromRuntime(
                    RuntimeExecutionErrorCode.NavigationFailed,
                    "Navigation failed.")
            };
        }
    }
}
