param(
	[string]$ConnectionString = "Server=JHB-BASIL\\SQLEXPRESS01;Database=BookingSystem;Trusted_Connection=True;MultipleActiveResultSets=true;",
	[string]$MigrationName = "InitialCreate",
	[string]$InfrastructureProject = "BookingSystem_Explore.Infrastructure",
	[string]$StartupProject = "BookingSystem_Explore.Api"
)

Write-Host "Using connection string:`n$ConnectionString`n"

# Export env var for design-time factory and for EF CLI
$env:DefaultConnection = $ConnectionString

Write-Host "Restoring dotnet tools (if any)"
dotnet tool restore

Write-Host "Adding migration '$MigrationName' to project $InfrastructureProject"
dotnet ef migrations add $MigrationName --project $InfrastructureProject --startup-project $StartupProject -o Persistence/Migrations

Write-Host "Applying migrations to database using startup project $StartupProject"
dotnet ef database update --project $InfrastructureProject --startup-project $StartupProject

Write-Host "Migrations complete."
