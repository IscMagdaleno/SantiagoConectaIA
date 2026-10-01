using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.BackgroundServices
{
    public class DailyFacebookPublicacionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly ILogger<DailyFacebookPublicacionBackgroundService> _logger;

        public DailyFacebookPublicacionBackgroundService(
            IServiceProvider serviceProvider,
            IPublicacionAutomaticaService automatica,
            ILogger<DailyFacebookPublicacionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _automatica = automatica;
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
                    await Task.Delay(espera, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await PublicarNoticiaMasRecienteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la publicación automática de Facebook.");
                }
            }
        }

        private async Task PublicarNoticiaMasRecienteAsync(CancellationToken cancellationToken)
        {
            var estado = await _automatica.GetAsync(cancellationToken);
            if (!estado.bActivo)
            {
                _logger.LogInformation("La publicación automática de Facebook está desactivada. No se publica hoy.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var noticiasDomain = scope.ServiceProvider.GetRequiredService<INoticiasDomain>();
            var gemini = scope.ServiceProvider.GetRequiredService<IGeminiPublicacionService>();
            var facebook = scope.ServiceProvider.GetRequiredService<IFacebookPublishService>();

            var respuesta = await noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = true });
            if (!respuesta.IsSuccess || respuesta.Data == null)
            {
                _logger.LogWarning("No se pudo consultar las noticias para la publicación automática: {Mensaje}", respuesta.Message);
                return;
            }

            var noticia = respuesta.Data
                .OrderByDescending(n => n.dtFechaPublicacion)
                .ThenByDescending(n => n.iIdNoticia)
                .FirstOrDefault();

            if (noticia == null)
            {
                _logger.LogInformation("No hay noticias activas para publicar en Facebook.");
                return;
            }

            if (estado.iIdUltimaNoticia == noticia.iIdNoticia)
            {
                _logger.LogInformation(
                    "La noticia más reciente ({Id} - {Titulo}) ya se publicó en Facebook. No se envía de nuevo.",
                    noticia.iIdNoticia,
                    noticia.vchTitulo);
                return;
            }

            if (string.IsNullOrWhiteSpace(noticia.vchImagenPortada))
            {
                _logger.LogWarning(
                    "La noticia {Id} no tiene imagen de portada. No se publicó en Facebook.",
                    noticia.iIdNoticia);
                return;
            }

            _logger.LogInformation(
                "Redactando con IA la noticia {Id} ({Fecha:dd/MM/yyyy} - {Titulo}).",
                noticia.iIdNoticia,
                noticia.dtFechaPublicacion,
                noticia.vchTitulo);

            var mejorado = await gemini.MejorarAsync(new PostMejorarPublicacion
            {
                iIdNoticia = noticia.iIdNoticia,
                vchTitulo = noticia.vchTitulo,
                nvchContenido = noticia.nvchContenido
            }, cancellationToken);

            if (!mejorado.IsSuccess || string.IsNullOrWhiteSpace(mejorado.Data))
            {
                _logger.LogWarning("Gemini no redactó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, mejorado.Message);
                return;
            }

            var publicado = await facebook.PublicarAsync(new PostPublicarFacebook
            {
                Message = mejorado.Data,
                ImageUrl = noticia.vchImagenPortada
            }, cancellationToken);

            if (!publicado.IsSuccess)
            {
                _logger.LogWarning("Make no aceptó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, publicado.Message);
                return;
            }

            await _automatica.RegistrarPublicacionAsync(noticia.iIdNoticia, noticia.vchTitulo, cancellationToken);
            _logger.LogInformation("Noticia {Id} publicada en Facebook.", noticia.iIdNoticia);
        }
    }
}
