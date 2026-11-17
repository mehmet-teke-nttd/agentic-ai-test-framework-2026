# Agentic AI Test Framework 2026

This repository contains a .NET-based agentic testing framework for UI, API, and E2E workflows using:

- intent -> contract -> execution architecture
- registry-driven POM patterns (`config/ui`, `config/api`)
- Reqnroll + NUnit + Playwright test execution
- failure analysis, bug draft generation, and loop-based quality workflows

## Project Layout

- `src/AgenticQa.Core/` - core framework engine and services
- `tests/AgenticQa.BddTests/` - BDD/E2E scenarios and hooks
- `config/ui/` - page, element, and component registries
- `config/api/` - API endpoint registry
- `artifacts/` - generated outputs (results, screenshots, analysis, agent-runs)
- `.cursor/agents/` - QA orchestrator and specialist agent roles
- `.cursor/rules/` - framework, Playwright, test generation, security, defect policies
- `.cursor/commands/` - thin workflow entry points (`/generate-test`, `/run-test`, etc.)
- `.cursor/skills/` - project skills (including quality, bug, and smoke loops)

## Common Commands

- Run all tests:
  - `dotnet test`
- Run smoke tests:
  - `dotnet test --filter "TestCategory=smoke"`
- Run mandatory smoke gate:
  - `./scripts/run-mandatory-smoke.ps1`
- Validate agent-run artifacts:
  - `./scripts/validate-agent-runs.ps1 -RequireFiles`

## Configuration

- UI test base URL:
  - `tests/AgenticQa.BddTests/appsettings.json` (`testSettings.publicBaseUrl`; `testSettings.baseUrl` still supported for backward compatibility)
- Mock login app base path:
  - `tests/AgenticQa.BddTests/appsettings.json` (`testSettings.mockAppBasePath`; falls back to `browser-settings.json` `baseUrl`)
- Browser settings:
  - `tests/AgenticQa.BddTests/browser-settings.json`
- Login credentials:
  - Environment variables `QA_USERNAME` and `QA_PASSWORD` are required for login scenarios.

## Agentic QA in Cursor

- Coordinator: `.cursor/agents/qa-orchestrator.md`
- Commands (project convention): `/generate-test`, `/run-test`, `/analyze-failure`, `/repair-locator`, `/create-bug` — see `.cursor/commands/`
- Skill index: `.cursor/skills/README.md`

## Notes

- `artifacts/` and build outputs are excluded from source control via `.gitignore`.
- Agent-run JSON format is documented in:
  - `docs/agent-runs-schema.md`

## Playwright MCP (Cursor)

This framework now includes workspace MCP config at `.cursor/mcp.json`:

- Server name: `playwright`
- Launch command: `npx -y @playwright/mcp@latest`

### One-time setup (Windows)

1. Install Node.js 18+ (includes `npm`/`npx`):
   - [https://nodejs.org/](https://nodejs.org/)
2. Reload Cursor in this workspace so MCP servers are re-discovered.
3. Use the agent with prompts like:
   - `Use Playwright MCP to open https://demo.playwright.dev/todomvc`

### Optional: install browser dependencies used by tests

For .NET Playwright test execution in this repo, install Chromium binaries if needed:

- `pwsh tests/AgenticQa.BddTests/bin/Debug/net10.0/playwright.ps1 install chromium`
