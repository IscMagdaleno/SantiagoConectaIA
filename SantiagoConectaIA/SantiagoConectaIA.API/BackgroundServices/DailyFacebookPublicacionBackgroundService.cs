using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
    public class DailyFacebookPublicacionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly PublicacionesBitacora _bitacora;
        private readonly ILogger<DailyFacebookPublicacionBackgroundService> _logger;

        public DailyFacebookPublicacionBackgroundService(
            IServiceProvider serviceProvider,
            IPublicacionAutomaticaService automatica,
            PublicacionesBitacora bitacora,
            ILogger<DailyFacebookPublicacionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _automatica = automatica;
            _bitacora = bitacora;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Publicación automática de Facebook iniciada.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var hora = await _automatica.HoraPublicacionAsync(stoppingToken);
                    var espera = HorarioPublicacion.TiempoHasta(hora);
                    _logger.LogInformation("Siguiente publicación automática de Facebook a las {Hora}:00, en {Espera}.", hora, espera);
                    _bitacora.Programar(PublicacionesBitacora.Noticias, DateTime.UtcNow.Add(espera));
                    await Task.Delay(espera, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var servicio = scope.ServiceProvider.GetRequiredService<INoticiaAutomaticaService>();
                    await servicio.PublicarMasRecienteAsync(manual: false, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la publicación automática de Facebook.");
                    _bitacora.Registrar(PublicacionesBitacora.Noticias, $"Error: {ex.Message}");
                }
            }
        }
    }
}
