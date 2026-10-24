# Post-deployment checks

## 1) Smoke test API
```powershell
./Deployment/SmokeTest.ps1 -ApiBaseUrl "https://localhost:7086"
```

## 2) Run all checks
```powershell
./Deployment/PostDeployChecks.ps1 -ApiBaseUrl "https://localhost:7086"
```

## 3) Notification behavior
- `GET /api/v1/appointments/{appointmentId}/notifications` requires `X-Management-Token` and returns appointment-specific notifications.

## Required headers for all `/api/*` requests
- `X-Client-Id`
- `Authorization: Bearer <server-issued-token>` obtained from `POST /api/v1/auth/token`
- `X-Session-Id`

Production must supply `ClientAccess:Jwt:SigningKey` and `Security:PhoneEncryption:Key` through environment variables, user secrets, or a managed secret store. They must not be placed in browser configuration or committed settings files.
