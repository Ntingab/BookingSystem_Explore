param(
	[string]$ApiBaseUrl = "https://localhost:7086",
	[string]$ClientId = "booking-ui"
)

$sessionId = [guid]::NewGuid().ToString("N")
$tokenResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/auth/token" -Method Post -ContentType "application/json" -Body (@{ clientId = $ClientId } | ConvertTo-Json)
$headers = @{
	"X-Client-Id" = $ClientId
	"X-Session-Id" = $sessionId
	"Authorization" = "Bearer $($tokenResponse.accessToken)"
}

Write-Host "Running API smoke checks..."
Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method Get | Out-Null
$branches = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/branches" -Method Get -Headers $headers
if (-not $branches -or $branches.Count -eq 0) { throw "No branches returned." }

$branchId = $branches[0].branchId
$branchServices = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/branches/$branchId/services" -Method Get -Headers $headers
if (-not $branchServices -or $branchServices.Count -eq 0) { throw "No branch services returned." }

$branchServiceId = $branchServices[0].branchServiceId
$date = (Get-Date).Date.AddDays(1).ToString("yyyy-MM-dd")
$slots = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/appointment-slots?branchServiceId=$branchServiceId&date=$date" -Method Get -Headers $headers
if (-not $slots -or $slots.Count -eq 0) { throw "No appointment slots returned." }

Write-Host "Smoke checks completed. Branches=$($branches.Count), Services=$($branchServices.Count), Slots=$($slots.Count)"
