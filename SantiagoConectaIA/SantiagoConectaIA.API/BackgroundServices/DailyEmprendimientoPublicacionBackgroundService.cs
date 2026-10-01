using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
    public class DailyEmprendimientoPublicacionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly ILogger<DailyEmprendimientoPublicacionBackgroundService> _logger;

        public DailyEmprendimientoPublicacionBackgroundService(
            IServiceProvider serviceProvider,
            IPublicacionAutomaticaService automatica,
            ILogger<DailyEmprendimientoPublicacionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _automatica = automatica;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Publicación automática de emprendimientos iniciada.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var hora = await _automatica.HoraEmprendimientosAsync(stoppingToken);
                    var espera = HorarioPublicacion.TiempoHasta(hora);
                    _logger.LogInformation("Siguiente publicación automática de emprendimientos a las {Hora}:00, en {Espera}.", hora, espera);
                    await Task.Delay(espera, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var servicio = scope.ServiceProvider.GetRequiredService<IEmprendimientoAutomaticoService>();
                    await servicio.PublicarSiguienteAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la publicación automática de emprendimientos.");
                }
            }
        }
    }
}
