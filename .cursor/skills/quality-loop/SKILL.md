---
name: quality-loop
description: Run a recurring quality loop over smoke and UI checks, contract validation, and failure triage. Use when the user asks for continuous branch quality checks, PR health monitoring, or a quality gate loop.
disable-model-invocation: true
---

# Quality Loop

## Goal
Keep the branch in a merge-ready quality state by repeatedly executing test and validation gates.

## Use With
- `run-ui-test`
- `run-smoke-gate`
- `validate-test-intent`
- `map-intent-to-contract`
- `analyze-failure`
- `publish-test-artifacts`
- `quality-gate-check`

## Single Iteration Workflow
1. Run `run-smoke-gate`.
2. Run `run-ui-test` for requested scope (`smoke`, `regression`, or feature tag).
3. Validate changed intents with `validate-test-intent`.
4. Validate intent-to-contract mapping with `map-intent-to-contract`.
5. If failures exist, run `analyze-failure`.
6. Publish artifacts with `publish-test-artifacts`.
7. Decide pass/fail using `quality-gate-check`.

## Loop Modes
- Fixed interval: every `N` minutes.
- Dynamic interval: shorter on failure, longer on success.

## Default Cadence
- On pass: next run in 30 minutes.
- On fail: next run in 5 minutes.

## Stop Conditions
- Manual stop request.
- `maxIterations` reached.
- `consecutivePassesRequired` reached (default `2`).

## Output Per Iteration
- loop iteration number
- gate result (`PASS` or `FAIL`)
- failure summary (if any)
- artifact paths
- next scheduled delay
