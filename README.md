# BookingSystem_Explore

## 1) Clone the repository

```powershell
git clone https://github.com/Ntingab/BookingSystem_Explore.git
cd BookingSystem_Explore
```

If Visual Studio currently has another repo open, use **File > Close Solution** first, then open this one.

## 2) Open the solution

Open this file in Visual Studio:

- `BookingSystem_Explore.slnx`

Or from terminal:

```powershell
start BookingSystem_Explore.slnx
```

## 3) Execute the SQL script (SQL Server)

Script path:

- `BookingSystem_Explore.Infrastructure/Migrations/20261017_MakeManagementTokenHashNotNull.sql`

### Option A: SSMS
1. Connect to your SQL Server instance.
2. Select the target database (example: `BookingSystem`).
3. Open the script file above.
4. Execute it.

### Option B: sqlcmd

```powershell
sqlcmd -S <SERVER_NAME> -d BookingSystem -E -i "BookingSystem_Explore.Infrastructure\Migrations\20261017_MakeManagementTokenHashNotNull.sql"
```

> Replace `<SERVER_NAME>` with your SQL Server instance (example: `JHB-BASIL\\SQLEXPRESS01`).

## 4) Run the API with HTTPS

### Visual Studio
1. Set startup project to `BookingSystem_Explore.Api`.
2. Select the **https** launch profile.
3. Run (F5 or Ctrl+F5).

### Command line

```powershell
dotnet run --project BookingSystem_Explore.Api --launch-profile https
```

Default URLs from launch settings:
- `https://localhost:7086`
- `http://localhost:5133`

Swagger UI:
- `https://localhost:7086/`

Health endpoint:
- `https://localhost:7086/health`

## 5) Run the API with Docker

### Visual Studio
1. Choose launch profile **Container (Dockerfile)**.
2. Run the project.

### Command line

```powershell
docker build -t bookingsystem-explore -f BookingSystem_Explore.Api/Dockerfile .
docker run --rm -p 8080:8080 -p 8081:8081 --name bookingsystem-explore bookingsystem-explore
```

Container URLs:
- `http://localhost:8080`
- `https://localhost:8081`

Swagger (container):
- `http://localhost:8080/`
