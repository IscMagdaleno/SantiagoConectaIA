using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SantiagoConectaFront;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var baseAddress = builder.HostEnvironment.BaseAddress;

if (builder.HostEnvironment.IsDevelopment())
{
	builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["LOCAL_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });

}
else
{
	builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["PRODUCTION_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
