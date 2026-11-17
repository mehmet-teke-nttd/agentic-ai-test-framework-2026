---
name: analyze-failure
description: Classify failed or blocked test runs and produce structured failure analysis with recommended actions. Use when tests fail, steps are blocked, or triage is needed before bug drafting.
disable-model-invocation: true
---

# Analyze Failure

## Goal

Evidence-based triage; **failed test ≠ product bug**.

## Inputs

Collect systematically:

- Failed test/scenario name and Reqnroll step
- `dotnet test` stderr and NUnit failure message
- Execution result JSON: `artifacts/results/`
- Failure analysis history if present: `artifacts/failure-analysis/`
- Screenshot: `artifacts/screenshots/`
- Playwright trace if captured (project hooks focus on screenshots)
- Relevant Page Object, step definition, registry entry
- Test data and env: `QA_USERNAME`, `QA_PASSWORD`, `QA_REQUIRE_UI_SMOKE`, `browser-settings.json`, `appsettings.json`
- Optional: latest matching `artifacts/agent-runs/*.json`

## Workflow

1. Identify first failing or blocked step.
2. Extract: `errorCode`, keyword, target/locator, message, stack trace.
3. Eliminate categories using evidence (not confidence alone).
4. Assign **agent classification** (one primary):
   - `PRODUCT_BUG`, `AUTOMATION_BUG`, `LOCATOR_FAILURE`, `TEST_DATA`, `ENVIRONMENT`, `CONFIGURATION`, `NETWORK`, `API_FAILURE`, `FLAKY_TEST`, `REQUIREMENT_ISSUE`, `UNKNOWN`
5. Set **coreClassification** for artifacts/services:
   - `PRODUCT_BUG` → `PRODUCT_DEFECT`
   - `AUTOMATION_BUG`, `LOCATOR_FAILURE`, `FLAKY_TEST` (when automation-related) → `AUTOMATION_ISSUE`
   - `TEST_DATA` → `TEST_DATA_ISSUE`
   - `ENVIRONMENT`, `CONFIGURATION`, `NETWORK`, `API_FAILURE` → `ENVIRONMENT_ISSUE` (document nuance in evidence)
   - `REQUIREMENT_ISSUE` → `REQUIREMENT_ISSUE`
   - `UNKNOWN` → `UNKNOWN`
6. Recommended action (align with `FailureRecommendedAction` where possible):
   - `QA_REVIEW_REQUIRED`, `INVESTIGATE_AUTOMATION`, `CHECK_TEST_DATA`, `CHECK_ENVIRONMENT`, `REVIEW_REQUIREMENTS`, `NO_ACTION`
   - Locator failures: recommend `repair-locator` workflow
7. Reference or persist under `artifacts/failure-analysis/` when writing files.

## Output contract

```json
{
  "test": "",
  "status": "FAILED",
  "classification": "",
  "coreClassification": "",
  "confidence": 0.0,
  "evidence": [],
  "rootCause": "",
  "recommendedAction": ""
}
```

- `confidence`: numeric 0.0–1.0 (not a substitute for evidence).
- Escalation fields optional: `escalationRequired`, `escalationLevel`, `escalationReason` (see core `FailureEscalationPolicy`).

## Done criteria

- Classification explicit and justified.
- Recommended action executable (e.g. run `repair-locator`, fix test data, env check—not default to defect).

## Agent

`.cursor/agents/failure-analyzer.md`
