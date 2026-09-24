---
name: publish-test-artifacts
description: Collect and summarize test artifacts such as execution results, failure analysis files, and screenshots. Use after test runs to produce a consistent artifact bundle for review or bug drafting.
disable-model-invocation: true
---

# Publish Test Artifacts

## Goal
Produce a reliable artifact manifest for each test iteration.

## Default Artifact Sources
- `artifacts/results/`
- `artifacts/failure-analysis/`
- `artifacts/screenshots/`
- optional command output snippets

## Workflow
1. Verify artifact directories exist.
2. Enumerate files created or updated in current run window.
3. Build a manifest with:
   - relative path
   - artifact type
   - modified timestamp
4. Return a concise summary and include missing directories as warnings.

## Output Format
- `manifestPath` (optional if persisted)
- `artifactSummary`
- `artifacts` list
- `warnings`
