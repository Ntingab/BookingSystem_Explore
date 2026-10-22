param(
	[string]$ApiBaseUrl = "https://localhost:7086",
	[string]$ConnectionString = ""
)

Write-Host "Step 1: Running unit/integration tests"
dotnet test .\BookingSystem_Explore.slnx --configuration Release
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed." }

Write-Host "Step 2: Running API smoke tests"
& "$PSScriptRoot\SmokeTest.ps1" -ApiBaseUrl $ApiBaseUrl
if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed." }

if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
	Write-Host "Step 3: Running DB soft SELECT checks"
	if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
		$sql = Get-Content "$PSScriptRoot\SmokeSelect.sql" -Raw
		sqlcmd -S "(local)" -d "BookingSystem" -Q $sql | Out-Host
	} else {
		Write-Warning "sqlcmd not found. Run Deployment/SmokeSelect.sql manually."
	}
}

Write-Host "Step 4: Optional GC runtime check"
Write-Host "Use: dotnet-counters monitor --process-id <pid> System.Runtime"
