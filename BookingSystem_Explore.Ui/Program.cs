using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookingSystem_Explore.Ui;
using BookingSystem_Explore.Ui.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiSettings = new ApiClientSettings
{
    BaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7086/",
    ClientId = builder.Configuration["Api:ClientId"] ?? string.Empty,
    ClientSecret = builder.Configuration["Api:ClientSecret"] ?? string.Empty,
    PhoneEncryptionKey = builder.Configuration["Api:PhoneEncryptionKey"] ?? string.Empty,
    JwtSigningKey = builder.Configuration["Api:JwtSigningKey"] ?? string.Empty,
    JwtIssuer = builder.Configuration["Api:JwtIssuer"] ?? "BookingSystem_Explore.Ui",
    JwtAudience = builder.Configuration["Api:JwtAudience"] ?? "BookingSystem_Explore.Api",
    JwtExpiryMinutes = int.TryParse(builder.Configuration["Api:JwtExpiryMinutes"], out var expiry) ? expiry : 30
};

builder.Services.AddScoped(_ => apiSettings);
builder.Services.AddScoped(_ => new PhoneEncryptionClient(apiSettings.PhoneEncryptionKey));
builder.Services.AddScoped(_ => new JwtTokenClient(apiSettings));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) });
builder.Services.AddScoped<BookingApiClient>();

await builder.Build().RunAsync();
