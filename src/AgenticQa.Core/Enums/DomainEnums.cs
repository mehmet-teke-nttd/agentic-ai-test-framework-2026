using System.Text.Json.Serialization;

namespace AgenticQa.Core.Enums;

public enum TestType
{
    [JsonStringEnumMemberName("UI-Functional-Positive")]
    UiFunctionalPositive,

    [JsonStringEnumMemberName("UI-Functional-Negative")]
    UiFunctionalNegative,

    [JsonStringEnumMemberName("API-Functional-Positive")]
    ApiFunctionalPositive,

    [JsonStringEnumMemberName("API-Functional-Negative")]
    ApiFunctionalNegative
}

public enum ValidationStatus
{
    [JsonStringEnumMemberName("VALID")]
    Valid,

    [JsonStringEnumMemberName("NEEDS_CLARIFICATION")]
    NeedsClarification
}

public enum ClarificationIssueType
{
    [JsonStringEnumMemberName("MISSING")]
    Missing,

    [JsonStringEnumMemberName("AMBIGUOUS")]
    Ambiguous,

    [JsonStringEnumMemberName("CONFLICTING")]
    Conflicting
}

public enum ClarificationOutcome
{
    [JsonStringEnumMemberName("CLARIFICATION")]
    Clarification,

    [JsonStringEnumMemberName("APPROVED_ASSUMPTION")]
    ApprovedAssumption,

    [JsonStringEnumMemberName("REJECTED_ASSUMPTION")]
    RejectedAssumption
}

public enum ExecutionKeyword
{
    [JsonStringEnumMemberName("NAVIGATE")]
    Navigate,

    [JsonStringEnumMemberName("CLICK")]
    Click,

    [JsonStringEnumMemberName("FILL")]
    Fill,

    [JsonStringEnumMemberName("SELECT")]
    Select,

    [JsonStringEnumMemberName("CHECK")]
    Check,

    [JsonStringEnumMemberName("VERIFY_VISIBLE")]
    VerifyVisible,

    [JsonStringEnumMemberName("VERIFY_TEXT")]
    VerifyText,

    [JsonStringEnumMemberName("VERIFY_URL")]
    VerifyUrl
}

public enum StepStatus
{
    [JsonStringEnumMemberName("PASSED")]
    Passed,

    [JsonStringEnumMemberName("FAILED")]
    Failed,

    [JsonStringEnumMemberName("BLOCKED")]
    Blocked,

    [JsonStringEnumMemberName("SKIPPED")]
    Skipped
}

public enum TestStatus
{
    [JsonStringEnumMemberName("PASSED")]
    Passed,

    [JsonStringEnumMemberName("FAILED")]
    Failed,

    [JsonStringEnumMemberName("BLOCKED")]
    Blocked
}

public enum LocatorType
{
    [JsonStringEnumMemberName("getByRole")]
    GetByRole,

    [JsonStringEnumMemberName("getByLabel")]
    GetByLabel,

    [JsonStringEnumMemberName("getByText")]
    GetByText,

    [JsonStringEnumMemberName("getByPlaceholder")]
    GetByPlaceholder,

    [JsonStringEnumMemberName("getByTestId")]
    GetByTestId,

    [JsonStringEnumMemberName("css")]
    Css
}

public enum LocatorValidationErrorCode
{
    [JsonStringEnumMemberName("TARGET_NOT_REGISTERED")]
    TargetNotRegistered,

    [JsonStringEnumMemberName("PAGE_MISMATCH")]
    PageMismatch,

    [JsonStringEnumMemberName("LOCATOR_NOT_FOUND")]
    LocatorNotFound,

    [JsonStringEnumMemberName("LOCATOR_NOT_UNIQUE")]
    LocatorNotUnique,

    [JsonStringEnumMemberName("INVALID_LOCATOR_CONFIG")]
    InvalidLocatorConfig
}

public enum SkipReason
{
    [JsonStringEnumMemberName("PREVIOUS_STEP_FAILED")]
    PreviousStepFailed,

    [JsonStringEnumMemberName("PREVIOUS_STEP_BLOCKED")]
    PreviousStepBlocked,

    [JsonStringEnumMemberName("DEPENDENCY_NOT_MET")]
    DependencyNotMet
}

public enum MappingStatus
{
    [JsonStringEnumMemberName("SUCCESS")]
    Success,

    [JsonStringEnumMemberName("MAPPING_FAILED")]
    MappingFailed
}

public enum ExecutionContractValidationErrorCode
{
    [JsonStringEnumMemberName("UNSUPPORTED_SCHEMA_VERSION")]
    UnsupportedSchemaVersion,

    [JsonStringEnumMemberName("MISSING_TEST_ID")]
    MissingTestId,

    [JsonStringEnumMemberName("NO_EXECUTION_STEPS")]
    NoExecutionSteps,

    [JsonStringEnumMemberName("INVALID_STEP_NUMBER")]
    InvalidStepNumber,

    [JsonStringEnumMemberName("DUPLICATE_STEP_NUMBER")]
    DuplicateStepNumber,

    [JsonStringEnumMemberName("NON_SEQUENTIAL_STEPS")]
    NonSequentialSteps,

    [JsonStringEnumMemberName("UNSUPPORTED_KEYWORD")]
    UnsupportedKeyword,

    [JsonStringEnumMemberName("MISSING_TARGET")]
    MissingTarget,

    [JsonStringEnumMemberName("MISSING_VALUE")]
    MissingValue,

    [JsonStringEnumMemberName("MISSING_EXPECTED")]
    MissingExpected,

    [JsonStringEnumMemberName("TEST_DATA_REFERENCE_NOT_FOUND")]
    TestDataReferenceNotFound,

    [JsonStringEnumMemberName("ACTION_MAPPING_NOT_FOUND")]
    ActionMappingNotFound
}

public enum ElementRegistryValidationErrorCode
{
    [JsonStringEnumMemberName("EMPTY_REGISTRY")]
    EmptyRegistry,

    [JsonStringEnumMemberName("MISSING_TARGET")]
    MissingTarget,

    [JsonStringEnumMemberName("DUPLICATE_TARGET")]
    DuplicateTarget,

    [JsonStringEnumMemberName("MISSING_PAGE")]
    MissingPage,

    [JsonStringEnumMemberName("INVALID_LOCATOR_TYPE")]
    InvalidLocatorType,

    [JsonStringEnumMemberName("MISSING_LOCATOR_VALUE")]
    MissingLocatorValue,

    [JsonStringEnumMemberName("INVALID_LOCATOR_CONFIG")]
    InvalidLocatorConfig
}

public enum LocatorValidationStatus
{
    [JsonStringEnumMemberName("VALID")]
    Valid,

    [JsonStringEnumMemberName("INVALID")]
    Invalid
}

public enum RuntimeExecutionErrorCode
{
    [JsonStringEnumMemberName("ASSERTION_FAILED")]
    AssertionFailed,

    [JsonStringEnumMemberName("PAGE_NOT_REGISTERED")]
    PageNotRegistered,

    [JsonStringEnumMemberName("NAVIGATION_FAILED")]
    NavigationFailed,

    [JsonStringEnumMemberName("PLAYWRIGHT_ACTION_FAILED")]
    PlaywrightActionFailed,

    [JsonStringEnumMemberName("TEST_DATA_REFERENCE_NOT_FOUND")]
    TestDataReferenceNotFound,

    [JsonStringEnumMemberName("UNSUPPORTED_HANDLER")]
    UnsupportedHandler
}

public enum TestExecutionErrorCode
{
    [JsonStringEnumMemberName("EXECUTION_CONTRACT_INVALID")]
    ExecutionContractInvalid,

    [JsonStringEnumMemberName("TEST_INTENT_INVALID")]
    TestIntentInvalid,

    [JsonStringEnumMemberName("EXECUTION_CONFIGURATION_ERROR")]
    ExecutionConfigurationError,

    [JsonStringEnumMemberName("EXECUTION_CANCELLED")]
    ExecutionCancelled
}

public enum FailureClassification
{
    [JsonStringEnumMemberName("PRODUCT_DEFECT")]
    ProductDefect,

    [JsonStringEnumMemberName("AUTOMATION_ISSUE")]
    AutomationIssue,

    [JsonStringEnumMemberName("TEST_DATA_ISSUE")]
    TestDataIssue,

    [JsonStringEnumMemberName("ENVIRONMENT_ISSUE")]
    EnvironmentIssue,

    [JsonStringEnumMemberName("REQUIREMENT_ISSUE")]
    RequirementIssue,

    [JsonStringEnumMemberName("UNKNOWN")]
    Unknown
}

public enum FailureRecommendedAction
{
    [JsonStringEnumMemberName("QA_REVIEW_REQUIRED")]
    QaReviewRequired,

    [JsonStringEnumMemberName("INVESTIGATE_AUTOMATION")]
    InvestigateAutomation,

    [JsonStringEnumMemberName("CHECK_TEST_DATA")]
    CheckTestData,

    [JsonStringEnumMemberName("CHECK_ENVIRONMENT")]
    CheckEnvironment,

    [JsonStringEnumMemberName("REVIEW_REQUIREMENTS")]
    ReviewRequirements,

    [JsonStringEnumMemberName("NO_ACTION")]
    NoAction
}

public enum FailureEscalationLevel
{
    [JsonStringEnumMemberName("NONE")]
    None,

    [JsonStringEnumMemberName("LOW")]
    Low,

    [JsonStringEnumMemberName("MEDIUM")]
    Medium,

    [JsonStringEnumMemberName("HIGH")]
    High,

    [JsonStringEnumMemberName("CRITICAL")]
    Critical
}

public enum FailureReviewDecision
{
    [JsonStringEnumMemberName("CONFIRMED")]
    Confirmed,

    [JsonStringEnumMemberName("REJECTED")]
    Rejected,

    [JsonStringEnumMemberName("NEEDS_MORE_INVESTIGATION")]
    NeedsMoreInvestigation
}

public enum BugRecommendationStatus
{
    [JsonStringEnumMemberName("BUG_DRAFT_RECOMMENDED")]
    BugDraftRecommended,

    [JsonStringEnumMemberName("NO_BUG")]
    NoBug,

    [JsonStringEnumMemberName("NO_BUG_YET")]
    NoBugYet
}

public enum BugSeverity
{
    [JsonStringEnumMemberName("LOW")]
    Low,

    [JsonStringEnumMemberName("MEDIUM")]
    Medium,

    [JsonStringEnumMemberName("HIGH")]
    High,

    [JsonStringEnumMemberName("CRITICAL")]
    Critical
}

public enum BugPriority
{
    [JsonStringEnumMemberName("UNASSIGNED")]
    Unassigned,

    [JsonStringEnumMemberName("LOW")]
    Low,

    [JsonStringEnumMemberName("MEDIUM")]
    Medium,

    [JsonStringEnumMemberName("HIGH")]
    High
}

public enum BugDraftReviewDecision
{
    [JsonStringEnumMemberName("APPROVED")]
    Approved,

    [JsonStringEnumMemberName("EDITED_AND_APPROVED")]
    EditedAndApproved,

    [JsonStringEnumMemberName("REJECTED")]
    Rejected
}

public enum BugDraftStatus
{
    [JsonStringEnumMemberName("DRAFT")]
    Draft,

    [JsonStringEnumMemberName("APPROVED")]
    Approved,

    [JsonStringEnumMemberName("REJECTED")]
    Rejected
}

public enum BugCreationStatus
{
    [JsonStringEnumMemberName("CREATED")]
    Created,

    [JsonStringEnumMemberName("BLOCKED")]
    Blocked,

    [JsonStringEnumMemberName("FAILED")]
    Failed,

    [JsonStringEnumMemberName("DUPLICATE")]
    Duplicate,

    [JsonStringEnumMemberName("READY_FOR_CREATION")]
    ReadyForCreation
}

public enum BugCreationErrorCode
{
    [JsonStringEnumMemberName("BUG_NOT_APPROVED")]
    BugNotApproved,

    [JsonStringEnumMemberName("AZURE_DEVOPS_CONFIGURATION_INVALID")]
    AzureDevOpsConfigurationInvalid,

    [JsonStringEnumMemberName("AZURE_DEVOPS_AUTHENTICATION_FAILED")]
    AzureDevOpsAuthenticationFailed,

    [JsonStringEnumMemberName("AZURE_DEVOPS_REQUEST_FAILED")]
    AzureDevOpsRequestFailed,

    [JsonStringEnumMemberName("AZURE_DEVOPS_FIELD_MAPPING_FAILED")]
    AzureDevOpsFieldMappingFailed,

    [JsonStringEnumMemberName("AZURE_DEVOPS_RESPONSE_INVALID")]
    AzureDevOpsResponseInvalid,

    [JsonStringEnumMemberName("DUPLICATE_BUG_FOUND")]
    DuplicateBugFound
}
