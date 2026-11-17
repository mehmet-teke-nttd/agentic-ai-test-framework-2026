# Project Skills (v2)

Workflow skills for the agentic QA framework. Conventions: YAML frontmatter with `name`, `description`, and `disable-model-invocation: true`.

## Primary workflow skills (orchestrated commands)

| Skill | Command | Agent |
|-------|---------|-------|
| `generate-test` | `/generate-test` | test-generator |
| `run-test` | `/run-test` | test-executor |
| `analyze-failure` | `/analyze-failure` | failure-analyzer |
| `repair-locator` | `/repair-locator` | locator-healer |
| `create-defect` | `/create-bug` | defect-agent |

Coordinator: `.cursor/agents/qa-orchestrator.md`. Rules: `.cursor/rules/`.

## Intent and contract pipeline

- `generate-test-intent`
- `validate-test-intent`
- `map-intent-to-contract`

Used by `generate-test` and quality loops.

## Execution and gates

- `run-ui-test` — UI-focused runs (called from `run-test`)
- `run-smoke-gate`
- `run-mandatory-smoke.ps1` (script)

## Triage and defects

- `analyze-failure` — evidence-based classification (agent + core enum mapping)
- `generate-bug-draft` — draft content (used by `create-defect`)
- `create-defect` — decision: CREATE / DRAFT / REJECT / NEEDS_REVIEW

## Loops and health

- `quality-loop`, `bug-loop`, `smoke-loop`
- `quality-gate-check`, `framework-health-check`
- `publish-test-artifacts`, `tag-and-prioritize-tests`

## Suggested end-to-end flow

1. `quality-loop` (or `smoke-loop` / `bug-loop`)
2. `generate-test` → `validate-test-intent` → `map-intent-to-contract` (when using core execution)
3. `run-test` or `run-smoke-gate`
4. `analyze-failure`
5. `repair-locator` (if locator/automation) **or** `create-defect` (if product evidence)
6. `publish-test-artifacts`

Legacy skill names remain valid; prefer **`generate-test`** and **`run-test`** as command-aligned entry points.
