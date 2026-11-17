---
name: run-test
description: Execute the smallest appropriate dotnet test scope for this repo and return structured results with artifact paths. Use when the user asks to run a test, feature, or category.
disable-model-invocation: true
---

# Run Test

## Goal

Run tests safely with repository-discovered paths and filters.

## Discovery

- UI BDD project: `tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj`
- Solution: `AgenticQaFramework.sln`
- Filters: NUnit `TestCategory` from Reqnroll tags (e.g. `smoke`, `login`, `mars`)

## Workflow

1. Identify test target (scenario name, tag, feature, or full project).
2. Build only when needed (`dotnet build` if binaries missing or code changed).
3. Execute smallest scope, for example:
   - All BddTests: `dotnet test tests/AgenticQa.BddTests/AgenticQa.BddTests.csproj`
   - Smoke: `--filter "TestCategory=smoke"`
   - Login: `--filter "TestCategory=login"`
   - Mars: `--filter "TestCategory=mars"`
   - Mandatory smoke gate: `./scripts/run-mandatory-smoke.ps1` (sets `QA_REQUIRE_UI_SMOKE=true`)
4. Capture stdout/stderr and pass/fail counts.
5. On failure or block, collect:
   - `artifacts/results/`
   - `artifacts/failure-analysis/`
   - `artifacts/screenshots/`
   - `artifacts/agent-runs/`
6. Return structured result (see below).

## UI-specific detail

For Playwright UI runs, follow skill `run-ui-test` (browser install, config paths, skip vs fail under smoke gate).

## Output contract

```json
{
  "test": "",
  "status": "PASSED|FAILED|BLOCKED|NOT_RUN",
  "duration": "",
  "error": null,
  "artifacts": []
}
```

## Agent

`.cursor/agents/test-executor.md`
