# Agentic AI Test Framework 2026

This repository contains a .NET-based agentic testing framework for UI, API, and E2E workflows using:

- intent -> contract -> execution architecture
- registry-driven POM patterns (`config/ui`, `config/api`)
- Reqnroll + NUnit + Playwright test execution
- failure analysis, bug draft generation, and loop-based quality workflows

## Project Layout

- `src/AgenticQa.Core/` - core framework engine and services
- `tests/AgenticQa.Core.UnitTests/` - unit tests
- `tests/AgenticQa.BddTests/` - BDD/E2E scenarios and hooks
- `config/ui/` - page, element, and component registries
- `config/api/` - API endpoint registry
- `artifacts/` - generated outputs (results, screenshots, analysis, agent-runs)
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
  - `tests/AgenticQa.BddTests/appsettings.json` (`testSettings.baseUrl`)
- Browser settings:
  - `tests/AgenticQa.BddTests/browser-settings.json`

## Notes

- `artifacts/` and build outputs are excluded from source control via `.gitignore`.
- Agent-run JSON format is documented in:
  - `docs/agent-runs-schema.md`
