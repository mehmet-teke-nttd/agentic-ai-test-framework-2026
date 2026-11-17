# /create-bug

Thin entry point: route to **QA Orchestrator** → **Defect Agent**.

## Usage

`/create-bug <test or failure context>`

## Execution

1. Ensure **`analyze-failure`** (or equivalent artifacts) exists.
2. Read `.cursor/agents/defect-agent.md`.
3. Follow skill **`create-defect`** (draft via **`generate-bug-draft`**).
4. Apply rules: **`defect-creation.mdc`**, **`security.mdc`**.

Default outcome is **DRAFT** or **NEEDS_REVIEW**, not automatic ADO submission.
