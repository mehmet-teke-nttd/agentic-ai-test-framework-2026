# Agentic AI Test Framework 2026

This repository contains a .NET test automation framework for UI, API, and E2E workflows with an agentic QA operating model:

- Intent -> contract -> execution architecture
- Registry-driven page/API metadata (`config/ui`, `config/api`)
- Reqnroll + NUnit + Playwright test execution
- Failure analysis and defect draft workflows
- Cursor command/agent orchestration under `.cursor/`

## Project Layout

- `src/AgenticQa.Core/` - framework engine, execution, analysis, and bug-drafting services
- `tests/AgenticQa.BddTests/` - BDD scenarios, hooks, step definitions, and page objects
- `config/ui/` - page, element, and component registries
- `config/api/` - API endpoint registry
- `artifacts/` - run artifacts (results, screenshots, analysis, agent-runs)
- `scripts/` - smoke gate and artifact validation scripts
- `.cursor/agents/` - QA orchestrator and specialist role prompts
- `.cursor/commands/` - workflow entry points (`/generate-test`, `/run-test`, etc.)
- `.cursor/skills/` - project skills supporting generation, execution, triage, and loops
- `.cursor/rules/` - QA framework, defect creation, Playwright, and security policies

## Prerequisites

- .NET SDK supporting `net10.0`
- Playwright browser binaries for local UI execution
- (Optional for Cursor MCP browser automation) Node.js 18+ for `npx`

## Common Commands

- Run all tests:
  - `dotnet test`
- Run smoke tests:
  - `dotnet test --filter "TestCategory=smoke"`
- Run login tests:
  - `dotnet test --filter "TestCategory=login"`
- Enforce mandatory UI smoke gate:
  - `./scripts/run-mandatory-smoke.ps1`
- Validate required agent-run artifacts:
  - `./scripts/validate-agent-runs.ps1 -RequireFiles`

If Playwright browser binaries are missing, install Chromium from the test output:

- `pwsh tests/AgenticQa.BddTests/bin/Debug/net10.0/playwright.ps1 install chromium`

## Configuration

- Test app and URLs:
  - `tests/AgenticQa.BddTests/appsettings.json`
  - `testSettings.publicBaseUrl`
  - `testSettings.mockAppBasePath`
  - `testSettings.baseUrl` (legacy/backward-compatible)
- Browser defaults:
  - `tests/AgenticQa.BddTests/browser-settings.json`

Environment variables:

- `QA_USERNAME` and `QA_PASSWORD` are required for login scenarios.
- `QA_REQUIRE_UI_SMOKE=true` can force mandatory smoke behavior in CI test commands.
- `QA_ALLOW_UI_SKIP=true` can allow UI scenario skipping in non-browser environments.

## Agentic QA in Cursor

- Coordinator: `.cursor/agents/qa-orchestrator.md`
- Primary commands:
  - `/generate-test`
  - `/run-test`
  - `/analyze-failure`
  - `/repair-locator`
  - `/create-bug`
- Skill index and workflow details: `.cursor/skills/README.md`

## Schema and Artifact Notes

- Agent-run JSON schema: `docs/agent-runs-schema.md`
- Agent architecture and hierarchy map: `docs/agent-architecture-map.md`
- Generated outputs (`artifacts/`, `TestResults/`, build output folders) are git-ignored by default.

## Playwright MCP (Cursor)

Workspace MCP is configured in `.cursor/mcp.json` using the `playwright` server (`npx -y @playwright/mcp@latest`).

One-time setup:

1. Install Node.js 18+ from [nodejs.org](https://nodejs.org/).
2. Reload Cursor so workspace MCP servers are discovered.
3. Use prompts such as: `Use Playwright MCP to open https://demo.playwright.dev/todomvc`.
