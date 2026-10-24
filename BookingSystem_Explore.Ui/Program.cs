using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookingSystem_Explore.Ui;
using BookingSystem_Explore.Ui.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var culture = new CultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var apiSettings = new ApiClientSettings
{
    BaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7086/",
    ClientId = builder.Configuration["Api:ClientId"] ?? string.Empty
};

builder.Services.AddScoped(_ => apiSettings);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) });
builder.Services.AddScoped<ApiAccessTokenClient>();
builder.Services.AddScoped<BookingApiClient>();

await builder.Build().RunAsync();
