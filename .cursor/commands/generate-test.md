# /generate-test

Thin entry point: route to **QA Orchestrator** → **Test Generator** agent.

## Usage

`/generate-test <requirement or story reference>`

## Execution

1. Read `.cursor/agents/qa-orchestrator.md` and `.cursor/agents/test-generator.md`.
2. Follow skill **`generate-test`** (reuse `generate-test-intent`, `validate-test-intent`, `map-intent-to-contract` as needed).
3. Apply rules: `test-generation`, `playwright`, `qa-framework`.

Do not embed the full workflow here—skills and agents own the procedure.
