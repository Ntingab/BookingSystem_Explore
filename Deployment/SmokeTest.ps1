param(
	[string]$ApiBaseUrl = "https://localhost:7086",
	[string]$ClientId = "booking-ui",
	[string]$ClientSecret = "Change-This-Client-Secret",
	[string]$Scope = "read write",
	[string]$JwtSigningKey = "Change-This-Jwt-Signing-Key-AtLeast-32-Bytes-Long-123456",
	[string]$JwtIssuer = "BookingSystem_Explore.Ui",
	[string]$JwtAudience = "BookingSystem_Explore.Api"
)

function New-JwtToken {
	param(
		[string]$SigningKey,
		[string]$Issuer,
		[string]$Audience,
		[string]$ScopeValue
	)

	$header = @{ alg = "HS256"; typ = "JWT" } | ConvertTo-Json -Compress
	$payload = @{
		iss = $Issuer
		aud = $Audience
		scope = $ScopeValue
		nbf = [int][double]::Parse((Get-Date -Date (Get-Date).ToUniversalTime() -UFormat %s))
		exp = [int][double]::Parse((Get-Date -Date (Get-Date).ToUniversalTime().AddMinutes(30) -UFormat %s))
	} | ConvertTo-Json -Compress

	function Convert-Base64Url([byte[]]$bytes) {
		[Convert]::ToBase64String($bytes).TrimEnd('=') -replace '\+', '-' -replace '/', '_'
	}

	$headerB64 = Convert-Base64Url ([Text.Encoding]::UTF8.GetBytes($header))
	$payloadB64 = Convert-Base64Url ([Text.Encoding]::UTF8.GetBytes($payload))
	$unsignedToken = "$headerB64.$payloadB64"

	$hmac = New-Object System.Security.Cryptography.HMACSHA256([Text.Encoding]::UTF8.GetBytes($SigningKey))
	$signature = Convert-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($unsignedToken)))
	return "$unsignedToken.$signature"
}

$sessionId = [guid]::NewGuid().ToString("N")
$token = New-JwtToken -SigningKey $JwtSigningKey -Issuer $JwtIssuer -Audience $JwtAudience -ScopeValue $Scope
$headers = @{
	"X-Client-Id" = $ClientId
	"X-Client-Secret" = $ClientSecret
	"X-Session-Id" = $sessionId
	"Authorization" = "Bearer $token"
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

$notifications = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/notifications" -Method Get -Headers $headers
Write-Host "Smoke checks completed. Branches=$($branches.Count), Services=$($branchServices.Count), Slots=$($slots.Count), Notifications=$($notifications.Count)"
