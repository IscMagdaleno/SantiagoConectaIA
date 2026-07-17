using EngramaCoreStandar.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Core.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository.AuthModule;

using SantiagoConectaIA.API.EngramaLevels.Domain.Core;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Servicios;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository;
using SantiagoConectaIA.API.SemanticKernel;
using SantiagoConectaIA.API.SemanticKernel.Agentes;
using SantiagoConectaIA.EngramaLevels.API.Infrastructure.Repository;
using SantiagoConectaIA.API.Middleware;
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models;
using SantiagoConectaIA.DAL.Provider;
using SantiagoConectaIA.API.BackgroundServices;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

using System.Reflection;
using SantiagoConectaIA.API.EngramaLevels.Domain.Core.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Core.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EventosModule;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register DbContext for EF Core
builder.Services.AddDbContext<EngramaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EngramaCloudConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// Ensure the AddEngramaDependenciesAPI method is defined in the above namespace
builder.Services.AddEngramaDependenciesAPI();

// JWT Config
var jwtSecret = builder.Configuration["JwtConfig:Secret"] ?? "SuperSecretKeyForSantiagoConectaIA123!@#";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"] ?? "SantiagoConectaIA",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtConfig:Audience"] ?? "SantiagoConectaIA",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<IAuthDomain, AuthDomain>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<ITramiteDominio, TramiteDominio>();
builder.Services.AddScoped<IOficinasDomain, OficinasDomain>();
builder.Services.AddScoped<IConversationalDominio, ConversationalDominio>();
builder.Services.AddScoped<IAzureBlobDomain, AzureBlobDomain>();
builder.Services.AddScoped<ILogsDomain, LogsDomain>();
builder.Services.AddScoped<INoticiasDomain, NoticiasDomain>();
builder.Services.AddScoped<IBuzonCiudadanoDomain, BuzonCiudadanoDomain>();
builder.Services.AddScoped<ICatalogosDomain, CatalogosDomain>();
builder.Services.AddScoped<IEmpresasDomain, EmpresasDomain>();
builder.Services.AddScoped<IEventosDomain, EventosDomain>();
builder.Services.AddScoped<IAnalyticsDomain, AnalyticsDomain>();
builder.Services.AddScoped<IPageVisitsDomain, PageVisitsDomain>();
builder.Services.AddScoped<IInformacionLocalDomain, InformacionLocalDomain>();

// WhatsApp Cloud API services

builder.Services.AddSingleton<WhatsAppMessageQueue>();
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
builder.Services.AddHostedService<WhatsAppWorker>();

builder.Services.AddScoped<ITramitesRepository, TramitesRepository>();
builder.Services.AddScoped<IOficinasRepository, OficinasRepository>();
builder.Services.AddScoped<IConversationalRepository, ConversationalRepository>();
builder.Services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();
builder.Services.AddScoped<ILogsRepository, LogsRepository>();
builder.Services.AddScoped<INoticiasRepository, NoticiasRepository>();
builder.Services.AddScoped<IBuzonCiudadanoRepository, BuzonCiudadanoRepository>();
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();
builder.Services.AddScoped<ICatalogosProvider, CatalogosProvider>();
builder.Services.AddScoped<IEmpresasRepository, EmpresasRepository>();
builder.Services.AddScoped<IEventosRepository, EventosRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IPageVisitsRepository, PageVisitsRepository>();
builder.Services.AddScoped<IInformacionLocalRepository, InformacionLocalRepository>();

builder.Services.AddScoped<IEngramaContextProcedures, EngramaContextProcedures>();
builder.Services.AddScoped<INoticiasScraperService, NoticiasScraperService>();
builder.Services.AddHostedService<DailyScraperBackgroundService>();


builder.Services.AddScoped<KernelProvider>();
builder.Services.AddScoped(sp => sp.GetRequiredService<KernelProvider>().GetKernel());


builder.Services.AddScoped<TramitesAgentes>();


builder.Services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();



/*Swagger configuration*/
builder.Services.AddSwaggerGen(options =>
{
	// using System.Reflection;
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var path = Path.Combine(AppContext.BaseDirectory, xmlFilename);
	options.IncludeXmlComments(path);

});




var app = builder.Build();





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(x => x
					.AllowAnyMethod()
					.AllowAnyHeader()
					.SetIsOriginAllowed(origin => true) // allow any origin
					.AllowCredentials());


app.UseHttpsRedirection();

app.UseMiddleware<ApiLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
