# /run-test

Thin entry point: route to **QA Orchestrator** → **Test Executor** agent.

## Usage

`/run-test [filter or scenario hint]`

Examples: `Login`, `TestCategory=smoke`, `TestCategory=login`

## Execution

1. Read `.cursor/agents/test-executor.md`.
2. Follow skill **`run-test`** (UI details via **`run-ui-test`**).
3. Return structured execution JSON from the skill output contract.

Do not modify failing tests in this command—report only.
