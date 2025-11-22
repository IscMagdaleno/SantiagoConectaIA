using EngramaCoreStandar.Extensions;

using SantiagoConectaIA.API.EngramaLevels.Domain.Core;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Domain.Servicios;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository;
using SantiagoConectaIA.API.SemanticKernel;
using SantiagoConectaIA.API.SemanticKernel.Agentes;
using SantiagoConectaIA.EngramaLevels.API.Infrastructure.Repository;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// Ensure the AddEngramaDependenciesAPI method is defined in the above namespace
builder.Services.AddEngramaDependenciesAPI();

builder.Services.AddScoped<ITramiteDominio, TramiteDominio>();
builder.Services.AddScoped<IOficinasDomain, OficinasDomain>();
builder.Services.AddScoped<IConversationalDominio, ConversationalDominio>();
builder.Services.AddScoped<IAzureBlobDomain, AzureBlobDomain>();

builder.Services.AddScoped<ITramitesRepository, TramitesRepository>();
builder.Services.AddScoped<IOficinasRepository, OficinasRepository>();
builder.Services.AddScoped<IConversationalRepository, ConversationalRepository>();
builder.Services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
