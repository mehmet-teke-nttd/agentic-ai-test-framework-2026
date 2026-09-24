---
name: analyze-failure
description: Classify failed or blocked test runs and produce structured failure analysis with recommended actions. Use when tests fail, steps are blocked, or triage is needed before bug drafting.
disable-model-invocation: true
---

# Analyze Failure

## Goal
Generate a reliable triage summary for failed or blocked executions.

## Inputs
- Execution result JSON from `artifacts/results/`
- Optional screenshot path from `artifacts/screenshots/`
- Optional logs or stderr snippets

## Workflow
1. Identify first failing or blocked step.
2. Extract:
   - `errorCode`
   - `keyword`
   - `target`
   - message
3. Classify into one of:
   - `PRODUCT_DEFECT`
   - `AUTOMATION_ISSUE`
   - `TEST_DATA_ISSUE`
   - `ENVIRONMENT_ISSUE`
   - `REQUIREMENT_ISSUE`
   - `UNKNOWN`
4. Set recommended action:
   - `QA_REVIEW_REQUIRED`
   - `INVESTIGATE_AUTOMATION`
   - `CHECK_TEST_DATA`
   - `CHECK_ENVIRONMENT`
   - `REVIEW_REQUIREMENTS`
   - `NO_ACTION`
5. Persist or reference history artifact under `artifacts/failure-analysis/`.

## Output Format
- classification
- confidence (`high`, `medium`, `low`)
- rootCauseSummary (2-4 lines)
- recommendedAction
- escalationRequired (`true` or `false`)
- escalationLevel (`NONE`, `LOW`, `MEDIUM`, `HIGH`, `CRITICAL`)
- escalationReason
- evidence paths

## Done Criteria
- Classification is explicit and justified by evidence.
- Recommended action is executable by owner team.
