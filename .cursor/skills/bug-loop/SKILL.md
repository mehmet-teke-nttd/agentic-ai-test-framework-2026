---
name: bug-loop
description: Run a recurring defect-triage loop for failing tests by rerunning failures, classifying causes, and producing bug drafts. Use when stabilizing a failing branch or converting repeated failures into actionable defects.
disable-model-invocation: true
---

# Bug Loop

## Goal
Continuously reduce failing tests by triaging and converting confirmed failures into bug drafts.

## Use With
- `run-ui-test`
- `analyze-failure`
- `generate-bug-draft`
- `tag-and-prioritize-tests`
- `publish-test-artifacts`

## Single Iteration Workflow
1. Run targeted tests for previously failed scenarios.
2. If failures remain, classify each with `analyze-failure`.
3. For product-defect-classified failures, run `generate-bug-draft`.
4. Apply risk tagging updates through `tag-and-prioritize-tests`.
5. Publish updated evidence with `publish-test-artifacts`.

## Prioritization Rules
- Prioritize `@Risk:High` and smoke-tagged failures first.
- Promote repeated failure (`>=3` iterations) to mandatory bug draft.
- Keep flaky patterns separate from confirmed defects.

## Stop Conditions
- No failed scenarios remain.
- Max iterations reached.
- Manual stop request.

## Output Per Iteration
- failed scenario count
- newly drafted bug count
- top 3 prioritized failures
- artifact paths
- next scheduled delay
