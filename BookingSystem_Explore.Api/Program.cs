global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Basic API setup so the project builds. Further configuration is done elsewhere in the solution.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// No Swagger here to keep the project build self-contained in this environment.

app.MapControllers();

app.Run();
