using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using SantiagoConectaIA.PWA;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Areas.ConversationalArea.Utiles;
using SantiagoConectaIA.PWA.Areas.OficinasArea.Utiles;
using SantiagoConectaIA.PWA.Areas.MockupArea.Utiles;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using SantiagoConectaIA.PWA.Areas.PageVisitsArea.Utiles;
using SantiagoConectaIA.PWA.Helpers;
using SantiagoConectaIA.PWA.Shared.Workspace;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var apiUrl = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7196/";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });


builder.Services.AddMudServices();


builder.Services.AddScoped<LoadingState>();
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddScoped<MapperHelper>();
builder.Services.AddScoped<IValidaServicioService, ValidaServicioService>();

builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<MainTramites>();
builder.Services.AddScoped<MainNoticias>();
builder.Services.AddScoped<MainConversational>();
builder.Services.AddScoped<MainOficinas>();
builder.Services.AddScoped<MainMockup>();
builder.Services.AddScoped<MainEmpresas>();
builder.Services.AddScoped<MainEventos>();
builder.Services.AddScoped<MainPageVisits>();

builder.Services.AddEngramaDependenciesBlazor();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
