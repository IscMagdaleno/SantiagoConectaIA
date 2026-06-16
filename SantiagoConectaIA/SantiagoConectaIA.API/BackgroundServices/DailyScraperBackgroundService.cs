using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.BackgroundServices
{
    public class DailyScraperBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyScraperBackgroundService> _logger;

        public DailyScraperBackgroundService(IServiceProvider serviceProvider, ILogger<DailyScraperBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scraper Background Service iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Ejecutar el scraping primero
                try
                {
                    _logger.LogInformation("Ejecutando extracción de noticias...");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scraperService = scope.ServiceProvider.GetRequiredService<INoticiasScraperService>();
                        await scraperService.RunScrapingAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al ejecutar el Scraping de noticias.");
                }

                // Determinar tiempo de espera basado en parámetros de BD
                int hoursDelay = 24;
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var catalogosDomain = scope.ServiceProvider.GetRequiredService<ICatalogosDomain>();
                        var reqParam = new PostGetParametro { vchAlias = "noticias.service" };
                        var resParam = await catalogosDomain.GetParametroByAlias(reqParam);
                        
                        if (resParam.IsSuccess && resParam.Data != null && !string.IsNullOrWhiteSpace(resParam.Data.NvchValor1))
                        {
                            if (int.TryParse(resParam.Data.NvchValor1, out int parsedHours) && parsedHours > 0)
                            {
                                hoursDelay = parsedHours;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo obtener el parámetro noticias.service, se usarán 24 horas por defecto.");
                }

                _logger.LogInformation($"Esperando {hoursDelay} hora(s) para la siguiente extracción...");
                await Task.Delay(TimeSpan.FromHours(hoursDelay), stoppingToken);
            }
        }
    }
}
