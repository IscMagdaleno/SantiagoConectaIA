using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SantiagoConectaFront;
using SantiagoConecta.SharedUI.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.JSInterop;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var baseAddress = builder.HostEnvironment.BaseAddress;

if (builder.HostEnvironment.IsDevelopment())
{
	builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["LOCAL_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });

}
else
{
	builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["PRODUCTION_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });
}

builder.Services.AddScoped<Data_Tramites>();
builder.Services.AddScoped<Data_Noticias>();
builder.Services.AddScoped<Data_Eventos>();
builder.Services.AddScoped<Data_BuzonCiudadano>();
builder.Services.AddScoped<Data_Emprendimientos>();
builder.Services.AddScoped<Data_Analytics>();
builder.Services.AddMudServices();


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var host = builder.Build();

var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
var result = await jsInterop.InvokeAsync<string>("blazorCulture.get");
CultureInfo culture;
if (result != null)
    culture = new CultureInfo(result);
else
    culture = new CultureInfo("es-MX");

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
