$ErrorActionPreference = "Stop"

$env:QA_REQUIRE_UI_SMOKE = "true"
dotnet test --filter "TestCategory=smoke"
