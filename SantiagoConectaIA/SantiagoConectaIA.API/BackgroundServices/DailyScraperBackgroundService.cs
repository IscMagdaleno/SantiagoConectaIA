using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
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
            _logger.LogInformation("Scraper Background Service iniciado. Se ejecutará cada 1 hora.");

            /* 
            // Ejecución Inmediata
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scraperService = scope.ServiceProvider.GetRequiredService<INoticiasScraperService>();
                    await scraperService.RunScrapingAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al ejecutar el Scraping Inmediato.");
            }
            */

            while (!stoppingToken.IsCancellationRequested)
            {
                // Esperar 1 hora antes de la siguiente ejecución
                _logger.LogInformation("Esperando 1 hora para la siguiente extracción...");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

                // Ejecutar el scraping
                try
                {
                    _logger.LogInformation("Ejecutando extracción horaria de noticias...");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scraperService = scope.ServiceProvider.GetRequiredService<INoticiasScraperService>();
                        await scraperService.RunScrapingAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al ejecutar el Scraping Horario.");
                }
            }
        }
    }
}
