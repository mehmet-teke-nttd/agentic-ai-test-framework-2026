param(
    [string]$ArtifactDirectory = "artifacts/agent-runs",
    [switch]$RequireFiles
)

$ErrorActionPreference = "Stop"

$requiredFields = @(
    "runId",
    "scenarioName",
    "tags",
    "testId",
    "startedAt",
    "completedAt",
    "outcome",
    "summary"
)

$allowedTestStatus = @("PASSED", "FAILED", "BLOCKED")
$allowedEscalationLevel = @("NONE", "LOW", "MEDIUM", "HIGH", "CRITICAL")

if (-not (Test-Path -LiteralPath $ArtifactDirectory)) {
    if ($RequireFiles) {
        Write-Error "Agent run directory does not exist: $ArtifactDirectory"
        exit 1
    }

    Write-Host "No agent run directory found at $ArtifactDirectory. Skipping validation."
    exit 0
}

$files = Get-ChildItem -LiteralPath $ArtifactDirectory -Filter "*.json" -File -ErrorAction Stop
if ($files.Count -eq 0) {
    if ($RequireFiles) {
        Write-Error "No agent run JSON files found in $ArtifactDirectory"
        exit 1
    }

    Write-Host "No agent run JSON files found in $ArtifactDirectory. Skipping validation."
    exit 0
}

$errors = New-Object System.Collections.Generic.List[string]

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    try {
        $obj = $content | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        $errors.Add("$($file.Name): invalid JSON - $($_.Exception.Message)")
        continue
    }

    foreach ($field in $requiredFields) {
        if (-not ($obj.PSObject.Properties.Name -contains $field)) {
            $errors.Add("$($file.Name): missing required field '$field'")
            continue
        }

        $value = $obj.$field
        if ($null -eq $value) {
            $errors.Add("$($file.Name): required field '$field' is null")
            continue
        }

        if ($field -ne "tags" -and [string]::IsNullOrWhiteSpace([string]$value)) {
            $errors.Add("$($file.Name): required field '$field' is empty")
        }
    }

    if ($obj.PSObject.Properties.Name -contains "tags") {
        if ($obj.tags -isnot [System.Array]) {
            $errors.Add("$($file.Name): 'tags' must be an array")
        }
        else {
            foreach ($tag in $obj.tags) {
                if ($tag -isnot [string]) {
                    $errors.Add("$($file.Name): each item in 'tags' must be a string")
                    break
                }
            }
        }
    }

    if ($obj.PSObject.Properties.Name -contains "confidence" -and $null -ne $obj.confidence) {
        [double]$confidence = 0
        if (-not [double]::TryParse([string]$obj.confidence, [ref]$confidence)) {
            $errors.Add("$($file.Name): 'confidence' must be a number")
        }
        elseif ($confidence -lt 0.0 -or $confidence -gt 1.0) {
            $errors.Add("$($file.Name): 'confidence' must be between 0 and 1")
        }
    }

    if ($obj.PSObject.Properties.Name -contains "testStatus" -and $null -ne $obj.testStatus) {
        if ($allowedTestStatus -notcontains [string]$obj.testStatus) {
            $errors.Add("$($file.Name): 'testStatus' must be one of: $($allowedTestStatus -join ', ')")
        }
    }

    if ($obj.PSObject.Properties.Name -contains "escalationLevel" -and $null -ne $obj.escalationLevel) {
        if ($allowedEscalationLevel -notcontains [string]$obj.escalationLevel) {
            $errors.Add("$($file.Name): 'escalationLevel' must be one of: $($allowedEscalationLevel -join ', ')")
        }
    }

    foreach ($dateField in @("startedAt", "completedAt")) {
        if ($obj.PSObject.Properties.Name -contains $dateField) {
            [datetimeoffset]$dt = [datetimeoffset]::MinValue
            if (-not [datetimeoffset]::TryParse([string]$obj.$dateField, [ref]$dt)) {
                $errors.Add("$($file.Name): '$dateField' must be a valid ISO-8601 date-time")
            }
        }
    }
}

if ($errors.Count -gt 0) {
    Write-Host "Agent run validation failed:" -ForegroundColor Red
    foreach ($errorLine in $errors) {
        Write-Host " - $errorLine" -ForegroundColor Red
    }
    exit 1
}

Write-Host "Validated $($files.Count) agent run record(s) successfully."
