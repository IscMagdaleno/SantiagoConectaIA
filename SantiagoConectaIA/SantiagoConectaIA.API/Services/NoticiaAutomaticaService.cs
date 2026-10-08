using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public interface INoticiaAutomaticaService
    {
        /// <summary>
        /// Publica en Facebook la noticia activa más reciente si todavía no se ha publicado. Con manual = true
        /// ignora el interruptor de la publicación diaria (lo usa el botón "Publicar siguiente").
        /// </summary>
        Task<Response<string>> PublicarMasRecienteAsync(bool manual = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve la noticia que toca publicar (la activa más reciente) validando que no se haya publicado ya
        /// y que tenga imagen de portada. El navegador la usa para preparar el post antes de publicarlo.
        /// </summary>
        Task<Response<Noticia>> GetSiguienteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Publica una noticia con el texto y la imagen ya preparados (post capturado). Solo acepta la noticia
        /// que devuelve GetSiguienteAsync y la registra como publicada.
        /// </summary>
        Task<Response<string>> PublicarGeneradaAsync(int idNoticia, string mensaje, string imagenUrl, CancellationToken cancellationToken = default);
    }

    public class NoticiaAutomaticaService : INoticiaAutomaticaService
    {
        /// <summary>Evita que la publicación diaria y el botón manual publiquen al mismo tiempo.</summary>
        private static readonly SemaphoreSlim Candado = new(1, 1);

        private readonly INoticiasDomain _noticiasDomain;
        private readonly IGeminiPublicacionService _gemini;
        private readonly IFacebookPublishService _facebook;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly PublicacionesBitacora _bitacora;
        private readonly ILogger<NoticiaAutomaticaService> _logger;

        public NoticiaAutomaticaService(
            INoticiasDomain noticiasDomain,
            IGeminiPublicacionService gemini,
            IFacebookPublishService facebook,
            IPublicacionAutomaticaService automatica,
            PublicacionesBitacora bitacora,
            ILogger<NoticiaAutomaticaService> logger)
        {
            _noticiasDomain = noticiasDomain;
            _gemini = gemini;
            _facebook = facebook;
            _automatica = automatica;
            _bitacora = bitacora;
            _logger = logger;
        }

        public async Task<Response<string>> PublicarMasRecienteAsync(bool manual = false, CancellationToken cancellationToken = default)
        {
            if (!await Candado.WaitAsync(0, cancellationToken))
            {
                return Anotar(Falla("Ya hay una publicación de noticias en curso. Espera a que termine."));
            }

            try
            {
                return Anotar(await PublicarAsync(manual, cancellationToken));
            }
            finally
            {
                Candado.Release();
            }
        }

        private async Task<Response<string>> PublicarAsync(bool manual, CancellationToken cancellationToken)
        {
            var estado = await _automatica.GetAsync(cancellationToken);
            if (!manual && !estado.bActivo)
            {
                _logger.LogInformation("La publicación automática de Facebook está desactivada. No se publica hoy.");
                return Falla("Desactivada: no se publicó.");
            }

            var candidata = await CandidataAsync(estado, cancellationToken);
            if (candidata.Error != null)
            {
                return Falla(candidata.Error);
            }

            var noticia = candidata.Noticia!;

            _logger.LogInformation(
                "Redactando con IA la noticia {Id} ({Fecha:dd/MM/yyyy} - {Titulo}).",
                noticia.iIdNoticia,
                noticia.dtFechaPublicacion,
                noticia.vchTitulo);

            var mejorado = await _gemini.MejorarAsync(new PostMejorarPublicacion
            {
                iIdNoticia = noticia.iIdNoticia,
                vchTitulo = noticia.vchTitulo,
                nvchContenido = noticia.nvchContenido
            }, cancellationToken);

            if (!mejorado.IsSuccess || string.IsNullOrWhiteSpace(mejorado.Data))
            {
                _logger.LogWarning("Gemini no redactó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, mejorado.Message);
                return Falla($"Gemini no redactó la noticia {noticia.iIdNoticia}: {mejorado.Message}");
            }

            var publicado = await _facebook.PublicarAsync(new PostPublicarFacebook
            {
                Message = mejorado.Data,
                ImageUrl = noticia.vchImagenPortada
            }, cancellationToken);

            if (!publicado.IsSuccess)
            {
                _logger.LogWarning("Make no aceptó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, publicado.Message);
                return Falla($"Make no aceptó la noticia {noticia.iIdNoticia}: {publicado.Message}");
            }

            await _automatica.RegistrarPublicacionAsync(noticia.iIdNoticia, noticia.vchTitulo, cancellationToken);
            _logger.LogInformation("Noticia {Id} publicada en Facebook.", noticia.iIdNoticia);
            return Exito($"Noticia #{noticia.iIdNoticia} ({noticia.vchTitulo}) publicada en Facebook.");
        }

        public async Task<Response<Noticia>> GetSiguienteAsync(CancellationToken cancellationToken = default)
        {
            var estado = await _automatica.GetAsync(cancellationToken);
            var candidata = await CandidataAsync(estado, cancellationToken);
            return candidata.Error == null
                ? new Response<Noticia> { IsSuccess = true, Data = candidata.Noticia!, Message = "Ok" }
                : Response<Noticia>.BadResult(candidata.Error, new Noticia());
        }

        public async Task<Response<string>> PublicarGeneradaAsync(int idNoticia, string mensaje, string imagenUrl, CancellationToken cancellationToken = default)
        {
            if (!await Candado.WaitAsync(0, cancellationToken))
            {
                return Anotar(Falla("Ya hay una publicación de noticias en curso. Espera a que termine."));
            }

            try
            {
                var estado = await _automatica.GetAsync(cancellationToken);
                var candidata = await CandidataAsync(estado, cancellationToken);
                if (candidata.Error != null)
                {
                    return Anotar(Falla(candidata.Error));
                }

                var noticia = candidata.Noticia!;
                if (noticia.iIdNoticia != idNoticia)
                {
                    return Anotar(Falla($"La noticia #{idNoticia} no es la que toca publicar (toca la #{noticia.iIdNoticia}). Vuelve a intentarlo."));
                }

                var publicado = await _facebook.PublicarAsync(new PostPublicarFacebook { Message = mensaje, ImageUrl = imagenUrl }, cancellationToken);
                if (!publicado.IsSuccess)
                {
                    _logger.LogWarning("Make no aceptó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, publicado.Message);
                    return Anotar(Falla($"Make no aceptó la noticia {noticia.iIdNoticia}: {publicado.Message}"));
                }

                await _automatica.RegistrarPublicacionAsync(noticia.iIdNoticia, noticia.vchTitulo, cancellationToken);
                _logger.LogInformation("Noticia {Id} publicada en Facebook con el post diseñado.", noticia.iIdNoticia);
                return Anotar(Exito($"Noticia #{noticia.iIdNoticia} ({noticia.vchTitulo}) publicada en Facebook."));
            }
            finally
            {
                Candado.Release();
            }
        }

        /// <summary>La noticia activa más reciente, o el motivo por el que no se puede publicar.</summary>
        private async Task<(Noticia? Noticia, string? Error)> CandidataAsync(PublicacionAutomatica estado, CancellationToken cancellationToken)
        {
            var respuesta = await _noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = true });
            if (!respuesta.IsSuccess || respuesta.Data == null)
            {
                _logger.LogWarning("No se pudo consultar las noticias para la publicación automática: {Mensaje}", respuesta.Message);
                return (null, $"No se pudieron consultar las noticias: {respuesta.Message}");
            }

            var noticia = respuesta.Data
                .OrderByDescending(n => n.dtFechaPublicacion)
                .ThenByDescending(n => n.iIdNoticia)
                .FirstOrDefault();

            if (noticia == null)
            {
                _logger.LogInformation("No hay noticias activas para publicar en Facebook.");
                return (null, "No hay noticias activas.");
            }

            if (estado.iIdUltimaNoticia == noticia.iIdNoticia)
            {
                _logger.LogInformation(
                    "La noticia más reciente ({Id} - {Titulo}) ya se publicó en Facebook. No se envía de nuevo.",
                    noticia.iIdNoticia,
                    noticia.vchTitulo);
                return (null, $"La noticia más reciente ya se publicó: #{noticia.iIdNoticia} {noticia.vchTitulo}. No hay una nueva.");
            }

            if (string.IsNullOrWhiteSpace(noticia.vchImagenPortada))
            {
                _logger.LogWarning("La noticia {Id} no tiene imagen de portada. No se publicó en Facebook.", noticia.iIdNoticia);
                return (null, $"La noticia #{noticia.iIdNoticia} no tiene imagen de portada; no se puede publicar.");
            }

            return (noticia, null);
        }
        private Response<string> Anotar(Response<string> resultado)
        {
            _bitacora.Registrar(PublicacionesBitacora.Noticias, resultado.Message);
            return resultado;
        }

        private static Response<string> Exito(string mensaje) => new() { IsSuccess = true, Data = mensaje, Message = mensaje };

        private static Response<string> Falla(string mensaje) => Response<string>.BadResult(mensaje, string.Empty);
    }
}
