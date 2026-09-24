---
name: smoke-loop
description: Run a recurring strict smoke gate and fail fast on runtime or environment issues. Use when monitoring UI test environment health, release readiness, or CI stability.
disable-model-invocation: true
---

# Smoke Loop

## Goal
Provide early detection of environment and runtime regressions through strict smoke execution.

## Use With
- `run-smoke-gate`
- `run-ui-test`
- `analyze-failure`
- `framework-health-check`
- `publish-test-artifacts`

## Single Iteration Workflow
1. Run `framework-health-check`.
2. Run strict smoke gate (`QA_REQUIRE_UI_SMOKE=true`) via `run-smoke-gate`.
3. Optionally run `run-ui-test` for smoke-only verification details.
4. If gate fails, run `analyze-failure`.
5. Publish artifacts with `publish-test-artifacts`.

## Cadence
- Recommended: every 10-15 minutes for active branches.
- For stable branches, every 30-60 minutes.

## Stop Conditions
- First failure (for alert mode).
- Manual stop.
- Max iterations reached.

## Output Per Iteration
- smoke gate status
- failing scenario names (if any)
- environment health summary
- artifact paths
- next run delay
