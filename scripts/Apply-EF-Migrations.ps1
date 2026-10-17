<#
PowerShell script to create and apply EF Core migration for Infrastructure project.
Adjust project paths and startup project as needed.
#>
param(
	[string]$MigrationName = "MakeManagementTokenHashNotNull",
	[string]$InfrastructureProject = "BookingSystem_Explore.Infrastructure",
	[string]$StartupProject = "BookingSystem_Explore.Api"
)

Write-Host "Creating migration '$MigrationName' in project $InfrastructureProject using startup project $StartupProject"

# Create migration
dotnet ef migrations add $MigrationName -p $InfrastructureProject -s $StartupProject

if ($LASTEXITCODE -ne 0) {
	Write-Error "Failed to add migration. Ensure EF tools are installed and projects build."
	exit $LASTEXITCODE
}

Write-Host "Applying migrations to database (update) using startup project $StartupProject"

# Apply migrations to database
dotnet ef database update -p $InfrastructureProject -s $StartupProject

if ($LASTEXITCODE -ne 0) {
	Write-Error "Failed to update database."
	exit $LASTEXITCODE
}

Write-Host "Migrations applied successfully."
