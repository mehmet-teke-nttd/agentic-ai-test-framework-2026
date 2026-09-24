---
name: run-ui-test
description: Execute UI BDD tests and report pass, fail, blocked, or skipped outcomes with artifact paths. Use when the user asks to run UI automation, login flow tests, or page validation scenarios.
disable-model-invocation: true
---

# Run UI Test

## Goal
Run UI scenarios and return actionable execution results.

## Preconditions
- `tests/AgenticQa.BddTests/appsettings.json` exists with test URL.
- Playwright browser binaries are installed.
- UI registries exist under `config/ui/`.

## Commands
- Run all BDD tests:
  - `dotnet test tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj`
- Run smoke:
  - `dotnet test tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj --filter "TestCategory=smoke"`
- Run mars scenario:
  - `dotnet test tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj --filter "TestCategory=mars"`

## Workflow
1. Validate required config files exist.
2. Run requested command.
3. Parse totals (passed/failed/skipped).
4. If failures or blocked results exist, collect artifact paths:
   - `artifacts/results/`
   - `artifacts/screenshots/`
   - `artifacts/failure-analysis/`
5. Return concise outcome plus next action.

## Failure Handling
- If Playwright is unavailable and strict smoke is not requested, report skip reason.
- If setup errors occur, include first root error and recommend exact remediation command.

## Output Contract
- `status`: `PASSED` | `FAILED` | `BLOCKED` | `SKIPPED`
- `testSummary`: counts
- `artifacts`: list of paths
- `nextAction`: one concrete recommended step
