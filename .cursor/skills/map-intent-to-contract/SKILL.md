---
name: map-intent-to-contract
description: Map a TestIntent into an ExecutionContract with deterministic execution steps. Use when the user wants runnable UI/API contract steps generated from intent actions.
disable-model-invocation: true
---

# Map Intent To Contract

## Goal
Convert a valid `TestIntent` into a valid `ExecutionContract`.

## Required Inputs
- A valid `TestIntent` JSON object
- Target layer (`UI` or `API`) inferred from `testType`

## Mapping Rules
1. Normalize incoming intent schema version (`0.9` or empty) to `1.0`.
2. Set `executionType`:
   - UI test types -> `UI`
   - API test types -> `API`
3. Preserve original `testId`.
4. Emit sequential `steps` with supported keywords only.
5. For UI login flow, use component targets:
   - `AuthForm.UsernameInput`
   - `AuthForm.PasswordInput`
   - `AuthForm.LoginButton`

## Known UI Mapping Pattern
- "Navigate to Login page" -> `NAVIGATE`, target `LoginPage`
- "Enter username" -> `FILL`, target `AuthForm.UsernameInput`, value `{{username}}`
- "Enter password" -> `FILL`, target `AuthForm.PasswordInput`, value `{{password}}`
- "Click Login button" -> `CLICK`, target `AuthForm.LoginButton`
- "Verify Dashboard is visible" -> `VERIFY_VISIBLE`, target `DashboardPage`

## Failure Behavior
If an action cannot be mapped deterministically:
- return status `MAPPING_FAILED`
- include issue entries with code `ACTION_MAPPING_NOT_FOUND`
- do not invent approximate target names

## Output Format
Return:
- mapping status (`SUCCESS` or `MAPPING_FAILED`)
- mapped `ExecutionContract` for success
- issue list for failure

## Done Criteria
- Output contract contains `schemaVersion: "1.0"`.
- Step numbering is sequential.
- No unsupported keyword is emitted.
