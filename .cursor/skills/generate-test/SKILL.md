---
name: generate-test
description: Turn requirements or acceptance criteria into TestIntent, validated contract-ready automation, and optional Reqnroll/Page Object changes. Use when the user asks to generate tests for a story or feature.
disable-model-invocation: true
---

# Generate Test

## Goal

Produce maintainable automation aligned with this repository, reusing existing assets.

## Workflow

Requirement → Acceptance criteria → Test intent → Ambiguity validation → Scenarios → Automation mapping → Page/registry mapping → Implementation → Validation

### Steps

1. **Search reuse**
   - Features: `tests/AgenticQa.BddTests/Features/`
   - Steps: `tests/AgenticQa.BddTests/StepDefinitions/`
   - Pages: `tests/AgenticQa.BddTests/Pages/`
   - Registries: `config/ui/`, `config/api/`
2. **Intent path** — follow skill `generate-test-intent`; then `validate-test-intent`; then `map-intent-to-contract` when execution-engine tests are in scope.
3. **BDD path** — when Reqnroll is appropriate, add or extend `.feature` files and step definitions; map tags (`@ui`, `@smoke`, etc.) for NUnit filters.
4. **Implementation** — match existing patterns (async steps, env-based credentials, semantic Playwright locators).
5. **Validation** — run targeted tests via skill `run-test` after code changes.

## Rules

- Apply `.cursor/rules/test-generation.mdc` and `playwright.mdc`.
- Report ambiguous requirements before finalizing automation.
- Do not duplicate scenarios or bindings that already exist.

## Related skills

- `generate-test-intent`, `validate-test-intent`, `map-intent-to-contract`, `tag-and-prioritize-tests`, `run-test`

## Agent

`.cursor/agents/test-generator.md`
