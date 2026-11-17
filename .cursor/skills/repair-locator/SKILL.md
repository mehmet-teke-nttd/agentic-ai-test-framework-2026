---
name: repair-locator
description: Diagnose broken Playwright locators in Page Objects or UI registries and propose or apply stable replacements. Use when failures point to missing or stale selectors.
disable-model-invocation: true
---

# Repair Locator

## Goal

Restore reliable locators without weakening assertions or adding arbitrary waits.

## Workflow

Broken locator → inspect DOM → identify candidates → rank by `.cursor/rules/playwright.mdc` → validate uniqueness → validate page mapping → propose replacement → optionally modify (if authorized) → run affected test via `run-test` → report result

### Steps

1. Locate source: `tests/AgenticQa.BddTests/Pages/*.cs` and/or `config/ui/element-registry.json`.
2. Use failure message, screenshot under `artifacts/screenshots/`, and Playwright MCP when available.
3. Prefer GetByRole, GetByLabel, GetByTestId, GetByPlaceholder, GetByText.
4. Confirm candidate is unique on the target page/component.
5. Update Page Object and registry together when both are in use for the same element.
6. Run smallest test filter (e.g. `TestCategory=login` for login flow).

## Do not

- Weaken assertions to pass.
- Add `Thread.Sleep` or un justified `Task.Delay`.
- Replace locators without semantic and stability justification.

## Output contract

```json
{
  "oldLocator": "",
  "proposedLocator": "",
  "strategy": "",
  "unique": false,
  "validated": false,
  "testResult": ""
}
```

## Agent

`.cursor/agents/locator-healer.md`
