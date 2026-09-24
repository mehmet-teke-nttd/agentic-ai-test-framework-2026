---
name: tag-and-prioritize-tests
description: Apply and validate test tags used for risk and execution prioritization. Use when organizing feature scenarios by regression scope, requirement mapping, and risk level.
disable-model-invocation: true
---

# Tag And Prioritize Tests

## Goal
Maintain consistent scenario tagging and produce execution priority groups.

## Expected Tags
- `@Regression`
- `@Feature:<name>`
- `@Risk:High|Medium|Low`
- `@Requirement:<workItemId>`
- optional execution tags (`@ui`, `@smoke`, `@login`, `@mars`)

## Workflow
1. Inspect `.feature` files and parse scenario tags.
2. Flag missing mandatory tags.
3. Build prioritized buckets:
   - Priority 1: smoke + high risk
   - Priority 2: regression + medium risk
   - Priority 3: remaining scenarios
4. Return filter examples for each bucket.

## Output Format
- `tagValidationSummary`
- `missingTagsByScenario`
- `priorityBuckets`
- `suggestedTestFilters`
