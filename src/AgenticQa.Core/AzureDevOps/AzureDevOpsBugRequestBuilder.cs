using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.AzureDevOps;

public sealed class AzureDevOpsBugRequestBuilder : IAzureDevOpsBugRequestBuilder
{
    private readonly AzureDevOpsOptions _options;
    private readonly IAzureDevOpsSeverityMapper _severityMapper;
    private readonly IAzureDevOpsPriorityMapper _priorityMapper;

    public AzureDevOpsBugRequestBuilder(
        IOptions<AzureDevOpsOptions> options,
        IAzureDevOpsSeverityMapper severityMapper,
        IAzureDevOpsPriorityMapper priorityMapper)
    {
        _options = options.Value;
        _severityMapper = severityMapper;
        _priorityMapper = priorityMapper;
    }

    public bool TryBuild(
        ApprovedBugDraft approvedDraft,
        out AzureDevOpsBugRequest request,
        out ExecutionError error)
    {
        request = new AzureDevOpsBugRequest();
        error = new ExecutionError();

        if (string.IsNullOrWhiteSpace(_options.OrganizationUrl) || string.IsNullOrWhiteSpace(_options.Project))
        {
            error = CreateError(BugCreationErrorCode.AzureDevOpsConfigurationInvalid, "Azure DevOps organization or project is not configured.");
            return false;
        }

        if (!_severityMapper.TryMap(approvedDraft.Draft.Severity, out var severity, out var severityError))
        {
            error = CreateError(BugCreationErrorCode.AzureDevOpsFieldMappingFailed, severityError);
            return false;
        }

        if (!_priorityMapper.TryMap(approvedDraft.Draft.Priority, out var priority, out var priorityError))
        {
            error = CreateError(BugCreationErrorCode.AzureDevOpsFieldMappingFailed, priorityError);
            return false;
        }

        var description = BuildDescription(approvedDraft);
        var reproSteps = BuildReproSteps(approvedDraft.Draft);
        var url = $"{_options.OrganizationUrl.TrimEnd('/')}/{_options.Project}/_apis/wit/workitems/$Bug?api-version={_options.ApiVersion}";

        request = new AzureDevOpsBugRequest
        {
            Url = url,
            Operations =
            [
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/System.Title", Value = approvedDraft.Draft.Title },
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/System.Description", Value = description },
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/Microsoft.VSTS.TCM.ReproSteps", Value = reproSteps },
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/Microsoft.VSTS.Common.Priority", Value = priority },
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/Microsoft.VSTS.Common.Severity", Value = severity },
                new AzureDevOpsJsonPatchOperation { Op = "add", Path = "/fields/System.Tags", Value = $"{approvedDraft.TestId};{approvedDraft.FailureClassification}" }
            ]
        };

        return true;
    }

    private static string BuildDescription(ApprovedBugDraft approvedDraft)
    {
        var draft = approvedDraft.Draft;
        return $"""
                <p><b>Summary</b>: {EscapeHtml(draft.Summary)}</p>
                <p><b>Test ID</b>: {EscapeHtml(draft.TestId)}</p>
                <p><b>Failure Classification</b>: {draft.FailureClassification}</p>
                <p><b>QA Review Decision</b>: {approvedDraft.Review.Decision}</p>
                <p><b>Environment</b>: {EscapeHtml(draft.Environment)}</p>
                """;
    }

    private static string BuildReproSteps(BugDraft draft)
    {
        var steps = string.Join(Environment.NewLine, draft.StepsToReproduce.Select((step, index) => $"{index + 1}. {step}"));
        return $"""
                {steps}

                Expected Result:
                {draft.ExpectedResult}

                Actual Result:
                {draft.ActualResult}
                """;
    }

    private static string EscapeHtml(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static ExecutionError CreateError(BugCreationErrorCode code, string message) =>
        new()
        {
            ErrorCode = code switch
            {
                BugCreationErrorCode.AzureDevOpsConfigurationInvalid => "AZURE_DEVOPS_CONFIGURATION_INVALID",
                BugCreationErrorCode.AzureDevOpsFieldMappingFailed => "AZURE_DEVOPS_FIELD_MAPPING_FAILED",
                _ => "AZURE_DEVOPS_REQUEST_FAILED"
            },
            Message = message
        };
}
