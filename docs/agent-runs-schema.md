# Agent Runs JSON Schema (v1)

This document defines the JSON shape written to:

- `artifacts/agent-runs/<runId>.json`
- machine-readable schema: `docs/agent-runs.schema.json`

## Purpose

Use this artifact as an execution ledger for scenario-level agent activity.  
It captures outcome, confidence, escalation, and evidence paths in one record.

## Top-Level Contract

```json
{
  "runId": "string (required)",
  "scenarioName": "string (required)",
  "tags": ["string", "string"],
  "testId": "string (required)",
  "testStatus": "PASSED|FAILED|BLOCKED (optional)",
  "startedAt": "ISO-8601 datetime (required)",
  "completedAt": "ISO-8601 datetime (required)",
  "outcome": "string (required)",
  "summary": "string (required)",
  "confidence": "number 0.0-1.0 (optional)",
  "escalationRequired": "boolean (optional)",
  "escalationLevel": "NONE|LOW|MEDIUM|HIGH|CRITICAL (optional)",
  "escalationReason": "string (optional)",
  "recommendedAction": "string (optional)",
  "resultFilePath": "string (optional)",
  "failureAnalysisPath": "string (optional)",
  "screenshotPath": "string (optional)"
}
```

## Field Notes

- `runId`: unique id for a single scenario run.
- `scenarioName`: human-readable scenario title.
- `tags`: scenario tags from feature metadata.
- `testId`: mapped test identifier from intent/execution result.
- `testStatus`: final framework status when available.
- `outcome`: high-level run outcome (for example `FAILED`, `PASSED_OR_SKIPPED`).
- `summary`: short narrative of the run result.
- `confidence`: failure-analysis confidence if analysis was executed.
- `escalationRequired`, `escalationLevel`, `escalationReason`: triage escalation signal.
- `recommendedAction`: remediation hint from failure analysis.
- path fields: absolute or relative artifact paths captured during run.

## Example Record

```json
{
  "runId": "20260924T152800123Z-successful-login-6f7f9e707a1c4d3dbf7f2d2112864a10",
  "scenarioName": "Successful login with valid credentials",
  "tags": ["ui", "smoke", "login"],
  "testId": "TC-LOGIN-001",
  "testStatus": "FAILED",
  "startedAt": "2026-09-24T15:27:55.3198821+00:00",
  "completedAt": "2026-09-24T15:28:00.0134869+00:00",
  "outcome": "FAILED",
  "summary": "Execution was blocked by locator or element configuration issues.",
  "confidence": 0.92,
  "escalationRequired": false,
  "escalationLevel": "NONE",
  "escalationReason": "No escalation required.",
  "recommendedAction": "InvestigateAutomation",
  "resultFilePath": "artifacts/results/TC-LOGIN-001-result.json",
  "failureAnalysisPath": "artifacts/failure-analysis/TC-LOGIN-001-analysis.json",
  "screenshotPath": "artifacts/screenshots/TC-LOGIN-001-blocked.png"
}
```

## Compatibility Guidance

- New optional fields can be added over time without breaking consumers.
- Consumers should ignore unknown fields and treat optional fields as nullable.

## CI Validation Command

Use the repository script to validate all generated agent-run files:

```powershell
./scripts/validate-agent-runs.ps1 -RequireFiles
```
