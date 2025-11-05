using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using SantiagoConectaIA.PWA;
using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Helpers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var url = "https://localhost:7196/";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(url) });


builder.Services.AddMudServices();

/*Engrama -> Services to call the API using the engrama Tools*/
builder.Services.AddScoped<LoadingState>();
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddScoped<MapperHelper>();
builder.Services.AddScoped<IValidaServicioService, ValidaServicioService>();
builder.Services.AddScoped<DataTramites>();

builder.Services.AddEngramaDependenciesBlazor();

await builder.Build().RunAsync();
