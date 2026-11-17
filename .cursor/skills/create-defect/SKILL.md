---
name: create-defect
description: Decide whether failure evidence supports a defect draft or creation; do not auto-file without policy. Use after analyze-failure when the user wants a bug from a test failure.
disable-model-invocation: true
---

# Create Defect

## Goal

Produce a review-ready defect or explicit `NEEDS_REVIEW` when evidence is insufficient.

## Workflow

Failure analysis → validate product bug → reproduce (when possible) → search duplicate (when ADO/checker configured) → gather evidence → draft defect → human review or policy check → create defect only if allowed

### Steps

1. Require output from skill `analyze-failure` or equivalent evidence from `artifacts/failure-analysis/` and `artifacts/results/`.
2. Apply `.cursor/rules/defect-creation.mdc`:
   - Do not create for automation/locator/test data/environment without QA confirmation.
3. Build draft using skill `generate-bug-draft` (title, environment, steps, expected/actual, evidence paths).
4. Map severity using execution impact; redact secrets per `security.mdc`.
5. **Decision**
   - `CREATE` — only if explicit policy + threshold met (product defect, confidence ≥ 0.95, reproduced, not duplicate, not flaky) and ADO not dry-run.
   - `DRAFT` — default for reviewable product issues.
   - `REJECT` — classification clearly non-product.
   - `NEEDS_REVIEW` — insufficient evidence.

Core services: `src/AgenticQa.Core/BugDrafting/`, Azure DevOps clients (PAT `AZURE_DEVOPS_PAT`, dry-run defaults).

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

## Agent

`.cursor/agents/defect-agent.md`

## Related skills

- `analyze-failure`, `generate-bug-draft`
