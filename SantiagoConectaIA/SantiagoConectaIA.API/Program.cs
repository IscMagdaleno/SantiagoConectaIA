using EngramaCoreStandar.Extensions;

using SantiagoConectaIA.API.EngramaLevels.Domain.Core;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Domain.Servicios;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository;
using SantiagoConectaIA.API.SemanticKernel;
using SantiagoConectaIA.API.SemanticKernel.Agentes;
using SantiagoConectaIA.EngramaLevels.API.Infrastructure.Repository;
using SantiagoConectaIA.API.Middleware;
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models;
using SantiagoConectaIA.DAL.Provider;

using System.Reflection;

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

builder.Services.AddScoped<ITramiteDominio, TramiteDominio>();
builder.Services.AddScoped<IOficinasDomain, OficinasDomain>();
builder.Services.AddScoped<IConversationalDominio, ConversationalDominio>();
builder.Services.AddScoped<IAzureBlobDomain, AzureBlobDomain>();
builder.Services.AddScoped<ILogsDomain, LogsDomain>();
builder.Services.AddScoped<INoticiasDomain, NoticiasDomain>();
builder.Services.AddScoped<IBuzonCiudadanoDomain, BuzonCiudadanoDomain>();

builder.Services.AddScoped<ITramitesRepository, TramitesRepository>();
builder.Services.AddScoped<IOficinasRepository, OficinasRepository>();
builder.Services.AddScoped<IConversationalRepository, ConversationalRepository>();
builder.Services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();
builder.Services.AddScoped<ILogsRepository, LogsRepository>();
builder.Services.AddScoped<INoticiasRepository, NoticiasRepository>();
builder.Services.AddScoped<IBuzonCiudadanoRepository, BuzonCiudadanoRepository>();
builder.Services.AddScoped<ICatalogosProvider, CatalogosProvider>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
