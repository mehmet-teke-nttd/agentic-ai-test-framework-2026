---
name: generate-test-intent
description: Generate a TestIntent JSON artifact from a requirement, user story, or acceptance criteria. Use when the user asks to create a new test case definition, test intent, or initial UI/API test plan in contract form.
disable-model-invocation: true
---

# Generate Test Intent

## Goal
Produce a valid `TestIntent` JSON document that is ready for mapping and execution.

## Required Inputs
- `testId`
- `title`
- `testType` (`UI-Functional-Positive`, `UI-Functional-Negative`, `API-Functional-Positive`, `API-Functional-Negative`)
- `preconditions` (list)
- `actions` (ordered list with step numbers)
- `expectedResults` (list)

## Rules
1. Always set `schemaVersion` to `1.0`.
2. Steps must start at `1` and be sequential.
3. Keep action text deterministic and mapper-friendly (for login: "Navigate to Login page", "Enter username", "Enter password", "Click Login button", "Verify Dashboard is visible").
4. Put sensitive values in placeholders in `testData` (for example `{{username}}`), not hardcoded credentials.
5. If inputs are incomplete, return explicit clarification questions before producing final JSON.

## Output Format
Return one JSON object with this shape:

```json
{
  "schemaVersion": "1.0",
  "testId": "TC-LOGIN-001",
  "title": "Successful login with valid credentials",
  "testType": "UI-Functional-Positive",
  "preconditions": ["User account exists"],
  "testData": {
    "username": "testuser",
    "password": "testpassword"
  },
  "actions": [
    { "step": 1, "action": "Navigate to Login page" }
  ],
  "expectedResults": ["Dashboard page is displayed"]
}
```

## Done Criteria
- JSON is syntactically valid.
- `schemaVersion` exists and is `1.0`.
- Actions are sequential and unambiguous.
- Contract is ready for `map-intent-to-contract`.
