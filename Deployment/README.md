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
- `GET /api/v1/notifications` returns all notifications (requires client headers + session header).
- `GET /api/v1/appointments/{appointmentId}/notifications` requires `X-Management-Token` and returns appointment-specific notifications.

## Required headers for all `/api/*` requests
- `X-Client-Id`
- `X-Client-Secret`
- `Authorization: Bearer <jwt-with-scope-claim>` (`read` for GET/HEAD/OPTIONS, `write` for create/update)
- `X-Session-Id`
