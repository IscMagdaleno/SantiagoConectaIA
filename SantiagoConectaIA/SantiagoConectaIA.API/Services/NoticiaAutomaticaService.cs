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
		/// Publica en Facebook la noticia más reciente que todavía no se haya publicado (según el registro en base
		/// de datos). Con manual = true ignora el interruptor de la publicación diaria (lo usa el botón
		/// "Publicar siguiente").
		/// </summary>
		Task<Response<string>> PublicarMasRecienteAsync(bool manual = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Devuelve la noticia que toca publicar (la más reciente sin publicar y con imagen de portada).
		/// El navegador la usa para preparar el post antes de publicarlo.
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

		/// <summary>Solo se publican noticias de los últimos días: una nota vieja ya no es "noticia".</summary>
		private const int DiasMaximosAntiguedad = 7;

		/// <summary>Una noticia que ya falló tantas veces en la publicación automática se salta para no atorar el día.</summary>
		private const int MaxFallosAutomaticosPorNoticia = 3;

		private readonly INoticiasDomain _noticiasDomain;
		private readonly IGeminiPublicacionService _gemini;
		private readonly IFacebookPublishService _facebook;
		private readonly IPublicacionAutomaticaService _automatica;
		private readonly IRegistroPublicacionesService _registro;
		private readonly PublicacionesBitacora _bitacora;
		private readonly ILogger<NoticiaAutomaticaService> _logger;

		public NoticiaAutomaticaService(
			INoticiasDomain noticiasDomain,
			IGeminiPublicacionService gemini,
			IFacebookPublishService facebook,
			IPublicacionAutomaticaService automatica,
			IRegistroPublicacionesService registro,
			PublicacionesBitacora bitacora,
			ILogger<NoticiaAutomaticaService> logger)
		{
			_noticiasDomain = noticiasDomain;
			_gemini = gemini;
			_facebook = facebook;
			_automatica = automatica;
			_registro = registro;
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

			var candidata = await CandidataAsync(manual, cancellationToken);
			if (candidata.Error != null)
			{
				return Falla(candidata.Error);
			}

			var noticia = candidata.Noticia!;
			var origen = manual ? PublicacionesTipos.Manual : PublicacionesTipos.Automatica;

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
				var mensaje = $"Gemini no redactó la noticia {noticia.iIdNoticia}: {mejorado.Message}";
				await RegistrarFalloAsync(noticia, origen, mensaje, cancellationToken);
				return Falla(mensaje);
			}

			var publicado = await _facebook.PublicarAsync(new PostPublicarFacebook
			{
				Message = mejorado.Data,
				ImageUrl = noticia.vchImagenPortada
			}, cancellationToken);

			if (!publicado.IsSuccess)
			{
				_logger.LogWarning("Make no aceptó la noticia {Id}: {Mensaje}", noticia.iIdNoticia, publicado.Message);
				var mensaje = $"Make no aceptó la noticia {noticia.iIdNoticia}: {publicado.Message}";
				await RegistrarFalloAsync(noticia, origen, mensaje, cancellationToken);
				return Falla(mensaje);
			}

			await RegistrarExitoAsync(noticia, origen, mejorado.Data, noticia.vchImagenPortada, cancellationToken);
			_logger.LogInformation("Noticia {Id} publicada en Facebook.", noticia.iIdNoticia);
			return Exito($"Noticia #{noticia.iIdNoticia} ({noticia.vchTitulo}) publicada en Facebook.");
		}

		public async Task<Response<Noticia>> GetSiguienteAsync(CancellationToken cancellationToken = default)
		{
			var candidata = await CandidataAsync(manual: true, cancellationToken);
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
				var candidata = await CandidataAsync(manual: true, cancellationToken);
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

				await RegistrarExitoAsync(noticia, PublicacionesTipos.Manual, mensaje, imagenUrl, cancellationToken);
				_logger.LogInformation("Noticia {Id} publicada en Facebook con el post diseñado.", noticia.iIdNoticia);
				return Anotar(Exito($"Noticia #{noticia.iIdNoticia} ({noticia.vchTitulo}) publicada en Facebook."));
			}
			finally
			{
				Candado.Release();
			}
		}

		/// <summary>
		/// La noticia activa más reciente que todavía no se ha publicado (según el registro) y que tiene imagen de
		/// portada, o el motivo por el que no hay ninguna. En la publicación manual no se descartan las que fallaron.
		/// </summary>
		private async Task<(Noticia? Noticia, string? Error)> CandidataAsync(bool manual, CancellationToken cancellationToken)
		{
			var respuesta = await _noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = true });
			if (!respuesta.IsSuccess || respuesta.Data == null)
			{
				_logger.LogWarning("No se pudo consultar las noticias para la publicación automática: {Mensaje}", respuesta.Message);
				return (null, $"No se pudieron consultar las noticias: {respuesta.Message}");
			}

			HashSet<int> publicadas;
			Dictionary<int, int> fallos;
			try
			{
				publicadas = await _registro.IdsPublicadosAsync(PublicacionesTipos.Noticia, cancellationToken);
				fallos = manual
					? new Dictionary<int, int>()
					: await _registro.FallosAutomaticosPorReferenciaAsync(PublicacionesTipos.Noticia, cancellationToken);
			}
			catch (Exception ex)
			{
				// Sin el registro no se sabe qué ya se publicó: es mejor no publicar que arriesgarse a repetir.
				_logger.LogError(ex, "No se pudo consultar el registro de publicaciones de noticias.");
				return (null, ex.Message);
			}

			var limite = DateTime.UtcNow.AddDays(-DiasMaximosAntiguedad);
			var sinPublicar = respuesta.Data
				.Where(n => n.dtFechaPublicacion >= limite && !publicadas.Contains(n.iIdNoticia))
				.OrderByDescending(n => n.dtFechaPublicacion)
				.ThenByDescending(n => n.iIdNoticia)
				.ToList();

			if (sinPublicar.Count == 0)
			{
				_logger.LogInformation("No hay noticias nuevas por publicar (últimos {Dias} días).", DiasMaximosAntiguedad);
				return (null, $"No hay noticias nuevas por publicar (se consideran las de los últimos {DiasMaximosAntiguedad} días).");
			}

			var conImagen = sinPublicar.Where(n => !string.IsNullOrWhiteSpace(n.vchImagenPortada)).ToList();
			if (conImagen.Count == 0)
			{
				_logger.LogWarning("Hay {Cantidad} noticia(s) nueva(s) pero ninguna tiene imagen de portada.", sinPublicar.Count);
				return (null, $"Hay {sinPublicar.Count} noticia(s) nueva(s) pero ninguna tiene imagen de portada; no se pueden publicar.");
			}

			var noticia = conImagen.FirstOrDefault(n => !fallos.TryGetValue(n.iIdNoticia, out var intentos) || intentos < MaxFallosAutomaticosPorNoticia);
			if (noticia == null)
			{
				return (null, $"Las noticias nuevas ya fallaron {MaxFallosAutomaticosPorNoticia} veces al publicarse automáticamente. Revisa el registro de publicaciones.");
			}

			return (noticia, null);
		}

		private Task RegistrarExitoAsync(Noticia noticia, string origen, string mensaje, string? imagenUrl, CancellationToken cancellationToken)
		{
			return _registro.RegistrarAsync(new PostSavePublicacionRegistro
			{
				vchTipo = PublicacionesTipos.Noticia,
				iIdReferencia = noticia.iIdNoticia,
				vchTitulo = noticia.vchTitulo,
				vchOrigen = origen,
				bExitoso = true,
				nvchMensaje = mensaje,
				vchImagenUrl = imagenUrl
			}, cancellationToken);
		}

		/// <summary>Solo los fallos de la publicación automática quedan en el registro (los manuales se le muestran al usuario).</summary>
		private Task RegistrarFalloAsync(Noticia noticia, string origen, string motivo, CancellationToken cancellationToken)
		{
			if (origen != PublicacionesTipos.Automatica)
			{
				return Task.CompletedTask;
			}

			return _registro.RegistrarAsync(new PostSavePublicacionRegistro
			{
				vchTipo = PublicacionesTipos.Noticia,
				iIdReferencia = noticia.iIdNoticia,
				vchTitulo = noticia.vchTitulo,
				vchOrigen = origen,
				bExitoso = false,
				vchResultado = motivo.Length > 500 ? motivo[..500] : motivo
			}, cancellationToken);
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
