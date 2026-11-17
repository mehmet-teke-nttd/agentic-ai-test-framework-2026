# ReqnRoll Test Commands

- Run all tests:
  - `dotnet test`
- Run tagged smoke scenarios (ReqnRoll tag mapped to NUnit category):
  - `dotnet test --filter "TestCategory=smoke"`
  - Mandatory CI smoke (fail if Playwright/browser setup fails):
    - PowerShell: `$env:QA_REQUIRE_UI_SMOKE="true"; dotnet test --filter "TestCategory=smoke"`
- Run login scenarios:
  - `dotnet test --filter "TestCategory=login"`

Notes:
- ReqnRoll scenarios are tagged with `@ui`, `@smoke`, and `@login`.
- Layered registries are copied under `config/ui` and `config/api` in test output.
- Agent run records are written to `artifacts/agent-runs/<runId>.json` for each scenario execution.
- Agent run JSON contract is documented in `docs/agent-runs-schema.md`.
- Cursor Playwright MCP is configured at workspace-level in `.cursor/mcp.json` (server: `playwright`).
- Login scenarios require `QA_USERNAME` and `QA_PASSWORD` environment variables.
- Browser bootstrap failures fail scenarios by default. To opt into skipping UI scenarios in non-browser environments, set `QA_ALLOW_UI_SKIP=true`.
- Validate agent-run artifacts in CI:
  - `./scripts/validate-agent-runs.ps1 -RequireFiles`
- If Playwright browser binaries are missing, run:
  - `pwsh bin/Debug/net10.0/playwright.ps1 install chromium`
