---
name: generate-bug-draft
description: Create a structured bug draft from failure analysis and execution artifacts. Use when a failed or blocked test should be converted into a review-ready defect report.
disable-model-invocation: true
---

# Generate Bug Draft

## Goal
Produce a complete, reviewable bug draft with reproducible evidence.

## Required Inputs
- failure analysis output
- execution result JSON
- scenario/test metadata (`testId`, title, tags)
- evidence paths (screenshots, logs)

## Workflow
1. Confirm failure classification supports bug creation.
2. Build draft fields:
   - Title
   - Environment
   - Preconditions
   - Steps to reproduce
   - Actual result
   - Expected result
   - Impact
   - Suggested severity and priority
3. Attach evidence paths.
4. Persist draft in bug draft artifact location.
5. Return draft summary for human approval.

## Content Rules
- Use factual wording from execution evidence.
- Do not speculate root cause without evidence.
- Keep reproduction steps deterministic and numbered.
- Include `errorCode` and first failing step.

## Output Contract
- `status`: `BUG_DRAFT_RECOMMENDED` | `NO_BUG` | `NO_BUG_YET`
- `draftPath`: artifact path
- `bugDraft`: structured object
- `reviewChecklist`: minimal approver checklist

## Done Criteria
- Draft contains enough detail for direct issue filing.
- Evidence links/paths are valid and present.
