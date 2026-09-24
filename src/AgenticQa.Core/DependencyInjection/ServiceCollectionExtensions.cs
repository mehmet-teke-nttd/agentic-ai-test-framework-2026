using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.ApiRegistry;
using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Execution.Validation;
using AgenticQa.Core.FailureAnalysis;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Playwright;
using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticQa.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgenticQaPhase4Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string? componentRegistryPath = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(elementRegistryPath);

        services.AddSingleton<IElementRegistryValidator, ElementRegistryValidator>();
        services.AddSingleton<IElementRegistryLoader, ElementRegistryLoader>();
        services.AddSingleton<IComponentRegistryLoader, ComponentRegistryLoader>();

        services.AddSingleton(sp =>
        {
            var pageElementRegistry = sp.GetRequiredService<IElementRegistryLoader>().Load(elementRegistryPath);
            var componentRegistry = string.IsNullOrWhiteSpace(componentRegistryPath)
                ? null
                : sp.GetRequiredService<IComponentRegistryLoader>().Load(componentRegistryPath);
            var mergedRegistry = UiRegistryComposer.Compose(pageElementRegistry, componentRegistry);
            var validation = sp.GetRequiredService<IElementRegistryValidator>().Validate(mergedRegistry);
            if (!validation.IsValid)
            {
                throw new ElementRegistryConfigurationException("Composed UI registry is invalid.", validation.Errors);
            }

            return mergedRegistry;
        });

        services.AddSingleton<IElementRegistryService, ElementRegistryService>();
        services.AddSingleton<ILocatorResolver, PlaywrightLocatorResolver>();
        services.AddSingleton<ILocatorValidator, PlaywrightLocatorValidator>();

        return services;
    }

    public static IServiceCollection AddAgenticQaPhase5Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string pageRegistryPath,
        string? componentRegistryPath = null,
        string? baseUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(pageRegistryPath);

        services.AddAgenticQaPhase4Services(elementRegistryPath, componentRegistryPath);

        services.AddSingleton<IPageRegistry>(sp =>
            new PageRegistry(
                pageRegistryPath,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PageRegistry>>(),
                baseUrl));
        services.AddSingleton<ITestDataResolver, TestDataResolver>();
        services.AddSingleton<IPlaywrightAssertionService, PlaywrightAssertionService>();
        services.AddSingleton<IExecutionCommandHandlerResolver, ExecutionCommandHandlerResolver>();
        services.AddSingleton<IStepExecutor, StepExecutor>();

        services.AddSingleton<IExecutionCommandHandler, NavigateCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, ClickCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, FillCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, SelectCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, CheckCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, VerifyVisibleCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, VerifyTextCommandHandler>();
        services.AddSingleton<IExecutionCommandHandler, VerifyUrlCommandHandler>();
        services.AddSingleton<IExecutionContractMapper, AgenticQa.Core.Execution.Mapping.ExecutionContractMapper>();

        return services;
    }

    public static IServiceCollection AddAgenticQaPhase6Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string pageRegistryPath,
        string resultOutputDirectory,
        string? componentRegistryPath = null,
        string? apiEndpointRegistryPath = null,
        string? baseUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultOutputDirectory);

        services.AddAgenticQaPhase5Services(elementRegistryPath, pageRegistryPath, componentRegistryPath, baseUrl);
        if (!string.IsNullOrWhiteSpace(apiEndpointRegistryPath))
        {
            services.AddSingleton<IApiEndpointRegistryService>(sp =>
                new ApiEndpointRegistryService(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ApiEndpointRegistryService>>(),
                    apiEndpointRegistryPath));
        }
        services.AddSingleton<IAmbiguityDetector, NoOpAmbiguityDetector>();
        services.AddSingleton<ITestIntentValidator, TestIntentValidator>();
        services.AddSingleton<IExecutionStepValidator, ExecutionStepValidator>();
        services.AddSingleton<ITestDataReferenceValidator, TestDataReferenceValidator>();
        services.AddSingleton<IExecutionContractValidator, ExecutionContractValidator>();

        services.AddSingleton<ITestResultAggregator, TestResultAggregator>();
        services.AddSingleton<ITestResultWriter>(sp =>
            new JsonTestResultWriter(
                resultOutputDirectory,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<JsonTestResultWriter>>()));
        services.AddSingleton<IExecutionEngine, ExecutionEngine>();

        return services;
    }

    public static IServiceCollection AddAgenticQaPhase8Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string pageRegistryPath,
        string resultOutputDirectory,
        string failureAnalysisOutputDirectory,
        string? componentRegistryPath = null,
        string? apiEndpointRegistryPath = null,
        string? baseUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(failureAnalysisOutputDirectory);

        services.AddAgenticQaPhase6Services(
            elementRegistryPath,
            pageRegistryPath,
            resultOutputDirectory,
            componentRegistryPath,
            apiEndpointRegistryPath,
            baseUrl);

        services.AddSingleton<IAiProvider, NoOpAiProvider>();
        services.AddSingleton<IFailureAnalysisPromptBuilder, FailureAnalysisPromptBuilder>();
        services.AddSingleton<IFailureAnalysisResponseValidator, FailureAnalysisResponseValidator>();
        services.AddSingleton<IFailureAnalysisAgent, FailureAnalysisAgent>();
        services.AddSingleton<IFailureClassificationService, FailureClassificationService>();
        services.AddSingleton<IFailureAnalysisHistoryService>(sp =>
            new FailureAnalysisHistoryService(
                failureAnalysisOutputDirectory,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FailureAnalysisHistoryService>>()));

        return services;
    }

    public static IServiceCollection AddAgenticQaPhase9Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string pageRegistryPath,
        string resultOutputDirectory,
        string failureAnalysisOutputDirectory,
        string bugDraftOutputDirectory,
        string? componentRegistryPath = null,
        string? apiEndpointRegistryPath = null,
        string? baseUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(bugDraftOutputDirectory);

        services.AddAgenticQaPhase8Services(
            elementRegistryPath,
            pageRegistryPath,
            resultOutputDirectory,
            failureAnalysisOutputDirectory,
            componentRegistryPath,
            apiEndpointRegistryPath,
            baseUrl);

        services.AddSingleton<IBugRecommendationService, BugRecommendationService>();
        services.AddSingleton<IBugDraftBuilder, BugDraftBuilder>();
        services.AddSingleton<IBugDraftResponseValidator, BugDraftResponseValidator>();
        services.AddSingleton<IBugDraftAgent, BugDraftAgent>();
        services.AddSingleton<IBugDraftPersistenceService>(sp =>
            new BugDraftPersistenceService(
                bugDraftOutputDirectory,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BugDraftPersistenceService>>()));
        services.AddSingleton<IBugDuplicateChecker, NoOpBugDuplicateChecker>();
        services.AddSingleton<IBugTrackerClient, NoOpBugTrackerClient>();
        services.AddSingleton<BugDraftWorkflowService>();

        return services;
    }

    public static IServiceCollection AddAgenticQaPhase10Services(
        this IServiceCollection services,
        string elementRegistryPath,
        string pageRegistryPath,
        string resultOutputDirectory,
        string failureAnalysisOutputDirectory,
        string bugDraftOutputDirectory,
        string bugResultOutputDirectory,
        Action<AzureDevOpsOptions> configureAzureDevOps,
        string? componentRegistryPath = null,
        string? apiEndpointRegistryPath = null,
        string? baseUrl = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureAzureDevOps);
        ArgumentException.ThrowIfNullOrWhiteSpace(bugResultOutputDirectory);

        services.AddAgenticQaPhase9Services(
            elementRegistryPath,
            pageRegistryPath,
            resultOutputDirectory,
            failureAnalysisOutputDirectory,
            bugDraftOutputDirectory,
            componentRegistryPath,
            apiEndpointRegistryPath,
            baseUrl);

        services.Configure(configureAzureDevOps);
        services.AddHttpClient("AzureDevOps");
        services.AddSingleton<IAzureDevOpsAuthenticationProvider, AzureDevOpsAuthenticationProvider>();
        services.AddSingleton<IAzureDevOpsSeverityMapper, AzureDevOpsSeverityMapper>();
        services.AddSingleton<IAzureDevOpsPriorityMapper, AzureDevOpsPriorityMapper>();
        services.AddSingleton<IAzureDevOpsBugRequestBuilder, AzureDevOpsBugRequestBuilder>();
        services.AddSingleton<IBugCreationGuard, BugCreationGuard>();
        services.AddSingleton<IBugCreationResultPersistenceService>(_ =>
            new BugCreationResultPersistenceService(bugResultOutputDirectory));
        services.AddSingleton<IBugDuplicateChecker, AzureDevOpsBugDuplicateChecker>();
        services.AddSingleton<IBugTrackerClient, AzureDevOpsBugTrackerClient>();
        services.AddSingleton<IBugCreationService, BugCreationService>();

        return services;
    }
}
