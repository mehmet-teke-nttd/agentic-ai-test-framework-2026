---
name: validate-test-intent
description: Validate TestIntent contracts for schema compatibility, completeness, and ambiguity. Use when a test intent is created or modified before mapping and execution.
disable-model-invocation: true
---

# Validate Test Intent

## Goal
Ensure `TestIntent` objects are valid and mapper-ready.

## Checks
- `schemaVersion` supported (`1.0` or migratable legacy)
- required fields present (`testId`, `title`, `testType`, `actions`, `expectedResults`)
- sequential action steps (starting at `1`)
- no duplicate step numbers
- no empty action text

## Workflow
1. Parse the input `TestIntent`.
2. Normalize schema version where applicable.
3. Run validation checks.
4. Return one of:
   - `VALID`
   - `NEEDS_CLARIFICATION` with issue list

## Output Format
- `status`
- `issues` (with `field`, `type`, `message`)
- `normalizedSchemaVersion`
