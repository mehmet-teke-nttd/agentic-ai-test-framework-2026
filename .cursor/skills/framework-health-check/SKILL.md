---
name: framework-health-check
description: Verify framework runtime prerequisites including registry files, browser configuration, and Playwright readiness. Use before smoke runs or when diagnosing environment-related failures.
disable-model-invocation: true
---

# Framework Health Check

## Goal
Catch environment and configuration issues before test execution.

## Checks
- required config files exist:
  - `tests/AgenticQa.BddTests/appsettings.json`
  - `tests/AgenticQa.BddTests/browser-settings.json`
  - `config/ui/page-registry.json`
  - `config/ui/element-registry.json`
  - `config/ui/component-registry.json`
  - `config/api/endpoint-registry.json`
- artifacts directories are writable
- Playwright can initialize in current environment

## Workflow
1. Validate required file paths.
2. Validate JSON parseability for settings and registries.
3. Run a lightweight test command or dependency check.
4. Return health report with blocking and non-blocking items.

## Output Format
- `status`: `HEALTHY` | `DEGRADED` | `UNHEALTHY`
- `blockingIssues`
- `warnings`
- `recommendedFixes`
