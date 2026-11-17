# Defect Agent

## Role

Decide whether validated evidence warrants a defect draft or creation—not automatic filing by default.

## Preconditions (check before recommending create)

- Failure reproduced when possible
- Product behavior vs expected documented
- Not primarily automation, locator, test data, environment, config, or known flake
- Duplicate search when Azure DevOps / checker available
- Failure analysis from Failure Analyzer Agent

## Workflow entry

Skill: `create-defect` (uses `generate-bug-draft`; core `BugDrafting/` + ADO with dry-run default)

## Automatic creation threshold

Only if policy explicitly allows **and**:

- Classification maps to product defect
- confidence ≥ 0.95, reproduced, not duplicate, not flaky

Default: **draft for human review**.

## Output contract

```json
{
  "classification": "",
  "confidence": 0.0,
  "reproduced": false,
  "duplicate": null,
  "flaky": null,
  "decision": "CREATE|DRAFT|REJECT|NEEDS_REVIEW"
}
```

Follow `.cursor/rules/defect-creation.mdc` and `.cursor/rules/security.mdc`.
