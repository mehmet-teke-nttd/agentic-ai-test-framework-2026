using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.Execution.Mapping;

public sealed class ExecutionContractMapper : IExecutionContractMapper
{
    private readonly ILogger<ExecutionContractMapper> _logger;

    public ExecutionContractMapper(ILogger<ExecutionContractMapper> logger)
    {
        _logger = logger;
    }

    public ExecutionContractMappingResult Map(TestIntent testIntent)
    {
        ArgumentNullException.ThrowIfNull(testIntent);
        ContractSchemaMigration.Normalize(testIntent);

        _logger.LogInformation("Execution contract mapping started for TestId={TestId}", testIntent.TestId);

        var issues = new List<ExecutionContractMappingIssue>();
        var steps = new List<ExecutionStep>();

        foreach (var action in testIntent.Actions.OrderBy(x => x.Step))
        {
            if (TryMapAction(action, out var mappedStep))
            {
                steps.Add(mappedStep!);
                continue;
            }

            issues.Add(new ExecutionContractMappingIssue
            {
                Step = action.Step,
                Action = action.Action,
                ErrorCode = ExecutionContractValidationErrorCode.ActionMappingNotFound,
                Message = "No deterministic execution mapping exists for this action."
            });
        }

        var status = issues.Count == 0 ? MappingStatus.Success : MappingStatus.MappingFailed;

        _logger.LogInformation(
            "Execution contract mapping completed for TestId={TestId}. Status={Status}, IssueCount={IssueCount}",
            testIntent.TestId,
            status,
            issues.Count);

        if (issues.Count > 0)
        {
            return new ExecutionContractMappingResult
            {
                Status = MappingStatus.MappingFailed,
                TestId = testIntent.TestId,
                ExecutionContract = null,
                Issues = issues
            };
        }

        return new ExecutionContractMappingResult
        {
            Status = MappingStatus.Success,
            TestId = testIntent.TestId,
            ExecutionContract = new ExecutionContract
            {
                SchemaVersion = ContractSchemaVersions.Current,
                TestId = testIntent.TestId,
                ExecutionType = GetExecutionType(testIntent.TestType),
                Steps = steps
            },
            Issues = []
        };
    }

    private static bool TryMapAction(TestAction action, out ExecutionStep? mappedStep)
    {
        mappedStep = null;
        var normalized = NormalizeAction(action.Action);

        mappedStep = normalized switch
        {
            "navigate to login page" => new ExecutionStep
            {
                Step = action.Step,
                Keyword = ExecutionKeyword.Navigate,
                Target = "LoginPage"
            },
            "enter username" => new ExecutionStep
            {
                Step = action.Step,
                Keyword = ExecutionKeyword.Fill,
                Target = "AuthForm.UsernameInput",
                Value = "{{username}}"
            },
            "enter password" => new ExecutionStep
            {
                Step = action.Step,
                Keyword = ExecutionKeyword.Fill,
                Target = "AuthForm.PasswordInput",
                Value = "{{password}}"
            },
            "click login button" => new ExecutionStep
            {
                Step = action.Step,
                Keyword = ExecutionKeyword.Click,
                Target = "AuthForm.LoginButton"
            },
            "verify dashboard is visible" => new ExecutionStep
            {
                Step = action.Step,
                Keyword = ExecutionKeyword.VerifyVisible,
                Target = "DashboardPage"
            },
            _ => null
        };

        return mappedStep is not null;
    }

    private static string GetExecutionType(TestType testType) =>
        testType is TestType.ApiFunctionalPositive or TestType.ApiFunctionalNegative
            ? "API"
            : "UI";

    private static string NormalizeAction(string actionText) =>
        string.Join(
            " ",
            actionText
                .Trim()
                .ToLowerInvariant()
                .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
}
