# BookingSystem_Explore

## 1) Clone the repository

```powershell
git clone https://github.com/Ntingab/BookingSystem_Explore.git
cd BookingSystem_Explore
```

If Visual Studio currently has another repo open, use **File > Close Solution** first. Then open the database folder in File Explorer:

```powershell
explorer "C:\Work\BookingSystem_Explore\database"
```

Afterward, open this repository's solution.

## 2) Open the solution

Open this file in Visual Studio:

- `BookingSystem_Explore.slnx`

Or from terminal:

```powershell
start BookingSystem_Explore.slnx
```

## 3) Create or update the SQL Server database

For a new local database, run:

- `database/booking_data.sql`

The script creates the `BookingSystem` database and its tables, then adds repeatable sample data. It does not delete an existing database unless its `@AllowDestructiveReset` option is explicitly enabled.

For an older database where `Appointments.ManagementTokenHash` is still nullable, also run:

- `BookingSystem_Explore.Infrastructure/Migrations/20261017_MakeManagementTokenHashNotNull.sql`

### Option A: SSMS
1. Connect to your SQL Server instance.
2. Open the required script listed above.
3. Execute it.

### Option B: sqlcmd

```powershell
sqlcmd -S <SERVER_NAME> -E -i "database\booking_data.sql"
```

Replace `<SERVER_NAME>` with your local SQL Server instance.

## 4) Configure local development secrets

The repository does not contain a machine-specific connection string or production keys. In Visual Studio, right-click `BookingSystem_Explore.Api`, select **Manage User Secrets**, and add:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=<SERVER_NAME>;Database=BookingSystem;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

Development generates temporary JWT and phone-encryption keys when they are absent. Production must provide `ClientAccess:Jwt:SigningKey` and `Security:PhoneEncryption:Key` through a secure configuration provider. Never commit real connection strings, passwords, or keys.

## 5) Run the API with HTTPS

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

## 6) Run the API with Docker

### Visual Studio
1. Choose launch profile **Container (Dockerfile)**.
2. Run the project.
3. Use the container URL displayed by Visual Studio. Visual Studio supplies the development certificate and maps the published ports.

### Command line (HTTP development container)

Build the image:

```powershell
docker build -t bookingsystem-explore -f BookingSystem_Explore.Api/Dockerfile .
```

Create an environment file outside the repository containing `ASPNETCORE_ENVIRONMENT=Development` and `ConnectionStrings__DefaultConnection`. Then run:

```powershell
docker run --rm -p 8080:8080 --env-file C:\secure\booking-api.env --name bookingsystem-explore bookingsystem-explore
```

The command-line container URL and Swagger UI are `http://localhost:8080/`. Port `8081` requires explicitly mounting and configuring an HTTPS certificate; `EXPOSE` alone does not enable HTTPS.

When SQL Server runs on the Windows host, a Linux container cannot normally use the host's named instance with `Trusted_Connection=True`. Configure SQL Server on a fixed TCP port and use `host.docker.internal,<PORT>` with appropriate SQL authentication in the external environment file. Do not commit those credentials.

## 7) Run the Blazor WebAssembly UI

The solution now includes a UI project:

- `BookingSystem_Explore.Ui`

### Visual Studio
1. Set startup project to `BookingSystem_Explore.Ui`.
2. Run with the **https** profile.

### Command line

```powershell
dotnet run --project BookingSystem_Explore.Ui --launch-profile https
```

Default UI URL:
- `https://localhost:7025`

The UI calls the API at:
- `https://localhost:7086/`

If your API URL is different, update:
- `BookingSystem_Explore.Ui/wwwroot/appsettings.json`

For example, when the API is the command-line HTTP container above, run the UI with its **http** profile and set `Api:BaseUrl` to `http://localhost:8080/`. An HTTPS UI cannot call an HTTP API because browsers block mixed content. The API URL is used by the browser, so it must be a host-accessible published URL—not an internal Docker container name.

## 8) Run tests

Use Test Explorer in Visual Studio, or run:

```powershell
dotnet test BookingSystem_Explore.slnx
```

Application errors are written through Serilog to the console and the configured rolling text log. Business services log operation context and preserve original exceptions when rethrowing them.
