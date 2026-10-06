using EngramaCoreStandar.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

// Parámetros de configuración (tabla Parametros)
builder.Services.AddSingleton<IParametrosService, ParametrosService>();

// JWT Config: secreto, issuer y audience desde los parámetros jwt.secret / jwt.config
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IParametrosService>((options, parametros) =>
    {
        JwtParametros Jwt() => parametros.GetJwtAsync().GetAwaiter().GetResult();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (_, _, _, _) => new[] { JwtTokenFactory.Llave(Jwt()) },
            ValidateIssuer = true,
            IssuerValidator = (issuer, _, _) => issuer == Jwt().Issuer
                ? issuer
                : throw new SecurityTokenInvalidIssuerException($"Issuer inválido: {issuer}"),
            ValidateAudience = true,
            AudienceValidator = (audiences, _, _) => audiences.Contains(Jwt().Audience),
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
builder.Services.AddScoped<IInformacionLocalDomain, InformacionLocalDomain>();
builder.Services.AddScoped<IBuzonCiudadanoDomain, BuzonCiudadanoDomain>();
builder.Services.AddScoped<ICatalogosDomain, CatalogosDomain>();
builder.Services.AddScoped<IEmpresasDomain, EmpresasDomain>();
builder.Services.AddScoped<IEventosDomain, EventosDomain>();
builder.Services.AddScoped<IAnalyticsDomain, AnalyticsDomain>();
builder.Services.AddScoped<IPageVisitsDomain, PageVisitsDomain>();
builder.Services.AddScoped<IFeedDomain, FeedDomain>();
builder.Services.AddScoped<IEmprendimientosFeedDomain, EmprendimientosFeedDomain>();
builder.Services.AddScoped<ICiudadanoDomain, CiudadanoDomain>();
builder.Services.AddScoped<IOpinionDomain, OpinionDomain>();
builder.Services.AddScoped<IPublicacionesCiudadanoDomain, PublicacionesCiudadanoDomain>();
	builder.Services.AddScoped<IInformacionLocalDomain, InformacionLocalDomain>();

	// WhatsApp Analytics services
	builder.Services.AddScoped<IWhatsAppDomain, WhatsAppDomain>();

	// WhatsApp Cloud API services

builder.Services.AddSingleton<WhatsAppMessageQueue>();
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
builder.Services.AddHttpClient<IFacebookPublishService, FacebookPublishService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IGeminiPublicacionService, GeminiPublicacionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient<IGeminiImagenService, GeminiImagenService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddHttpClient<IGeminiEventoService, GeminiEventoService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});
builder.Services.AddSingleton<PublicacionesBitacora>();
builder.Services.AddSingleton<IPublicacionAutomaticaService, PublicacionAutomaticaService>();
builder.Services.AddScoped<IEmprendimientoAutomaticoService, EmprendimientoAutomaticoService>();
builder.Services.AddScoped<INoticiaAutomaticaService, NoticiaAutomaticaService>();
builder.Services.AddHostedService<WhatsAppWorker>();

builder.Services.AddScoped<ITramitesRepository, TramitesRepository>();
builder.Services.AddScoped<IOficinasRepository, OficinasRepository>();
builder.Services.AddScoped<IConversationalRepository, ConversationalRepository>();
builder.Services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();
builder.Services.AddScoped<ILogsRepository, LogsRepository>();
builder.Services.AddScoped<INoticiasRepository, NoticiasRepository>();
builder.Services.AddScoped<IInformacionLocalRepository, InformacionLocalRepository>();
builder.Services.AddScoped<IBuzonCiudadanoRepository, BuzonCiudadanoRepository>();
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();
builder.Services.AddScoped<ICatalogosProvider, CatalogosProvider>();
builder.Services.AddScoped<IEmpresasRepository, EmpresasRepository>();
builder.Services.AddScoped<IEventosRepository, EventosRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IPageVisitsRepository, PageVisitsRepository>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IEmprendimientosFeedRepository, EmprendimientosFeedRepository>();
builder.Services.AddScoped<ICiudadanoRepository, CiudadanoRepository>();
builder.Services.AddScoped<IOpinionRepository, OpinionRepository>();
builder.Services.AddScoped<IPublicacionesCiudadanoRepository, PublicacionesCiudadanoRepository>();
	builder.Services.AddScoped<IInformacionLocalRepository, InformacionLocalRepository>();

	// WhatsApp Analytics repository
	builder.Services.AddScoped<IWhatsAppRepository, WhatsAppRepository>();

	builder.Services.AddScoped<IEngramaContextProcedures, EngramaContextProcedures>();
builder.Services.AddScoped<INoticiasScraperService, NoticiasScraperService>();
builder.Services.AddHostedService<DailyScraperBackgroundService>();
builder.Services.AddHostedService<DailyFacebookPublicacionBackgroundService>();
builder.Services.AddHostedService<DailyEmprendimientoPublicacionBackgroundService>();


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
