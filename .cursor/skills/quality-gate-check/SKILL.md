---
name: quality-gate-check
description: Evaluate quality gate status from smoke, UI tests, contract checks, and lint outcomes. Use when deciding if a branch or PR is ready to proceed.
disable-model-invocation: true
---

# Quality Gate Check

## Goal
Return a binary gate decision with clear failure reasons.

## Required Inputs
- smoke gate result
- UI test summary
- intent validation result
- mapping/contract validation result
- optional lints

## Gate Policy (Default)
- Smoke gate must pass.
- No failed tests in required scope.
- No blocking contract/intention validation errors.

## Workflow
1. Evaluate each gate dimension.
2. Build pass/fail decision.
3. If failed, list top blocking causes in priority order.
4. Provide next action command.

## Output Format
- `gate`: `PASS` | `FAIL`
- `reasons`: ordered list
- `blockingItems`: list
- `recommendedNextAction`
