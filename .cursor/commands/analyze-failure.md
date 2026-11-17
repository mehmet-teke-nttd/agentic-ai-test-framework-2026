# /analyze-failure

Thin entry point: route to **QA Orchestrator** → **Failure Analyzer** agent.

## Usage

`/analyze-failure <test or scenario name>`

## Execution

1. Read `.cursor/agents/failure-analyzer.md`.
2. Follow skill **`analyze-failure`**; gather artifacts under `artifacts/`.
3. Apply rules: `defect-creation`, `qa-framework`.
4. Return structured failure analysis JSON; do not default to product defect.

If classification is locator-related, suggest **`/repair-locator`**—not **`/create-bug`**.
