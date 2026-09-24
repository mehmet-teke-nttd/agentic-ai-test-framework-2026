---
name: run-smoke-gate
description: Run mandatory smoke validation that fails fast on runtime or browser setup issues. Use when validating CI gate readiness or release-blocking sanity checks.
disable-model-invocation: true
---

# Run Smoke Gate

## Goal
Execute smoke scenarios as a hard quality gate.

## Command
PowerShell:

```powershell
$env:QA_REQUIRE_UI_SMOKE="true"; dotnet test tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj --filter "TestCategory=smoke"
```

## Behavior
1. Set `QA_REQUIRE_UI_SMOKE=true`.
2. Run smoke category only.
3. Treat browser initialization errors as test failures, not skips.
4. Return non-zero command result as gate failure.

## Reporting Format
- `gate`: `PASS` or `FAIL`
- `reason`: one sentence
- `failedScenarios`: list (if any)
- `recommendedFix`: exact next command or config fix

## Done Criteria
- Smoke output clearly indicates pass/fail.
- If fail, includes explicit reason and remediation.
