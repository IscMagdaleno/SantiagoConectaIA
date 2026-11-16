using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.MemesModule;
using SantiagoConectaIA.Share.PostModels.MemesModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
	public class MemeService : IMemeService
	{
		private const int LimiteDiarioPorDefecto = 10;

		// Gemini tarda hasta ~1 minuto por imagen: se limitan las creaciones simultáneas para no saturar la cuota.
		private static readonly SemaphoreSlim Simultaneas = new(2, 2);

		private readonly IMemePlantillasClient _plantillas;
		private readonly IGeminiImagenService _imagenes;
		private readonly IMemesDomain _memes;
		private readonly IParametrosService _parametros;
		private readonly ILogger<MemeService> _logger;

		public MemeService(
			IMemePlantillasClient plantillas,
			IGeminiImagenService imagenes,
			IMemesDomain memes,
			IParametrosService parametros,
			ILogger<MemeService> logger)
		{
			_plantillas = plantillas;
			_imagenes = imagenes;
			_memes = memes;
			_parametros = parametros;
			_logger = logger;
		}

		public async Task<Response<ListaPlantillasMeme>> GetPlantillasAsync(PostGetPlantillasMeme post, CancellationToken cancellationToken = default)
		{
			try
			{
				post ??= new PostGetPlantillasMeme();
				var resultado = await _plantillas.ListarAsync(
					post.vchModo, post.vchBusqueda, post.vchCategoria, post.vchPeriodo,
					post.iLimite <= 0 ? 24 : post.iLimite, post.iDesde, cancellationToken);

				return resultado.Exitosa
					? new Response<ListaPlantillasMeme> { IsSuccess = true, Data = resultado.Lista, Message = "Ok" }
					: Response<ListaPlantillasMeme>.BadResult(resultado.Mensaje, new ListaPlantillasMeme());
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al consultar las plantillas de memes.");
				return Response<ListaPlantillasMeme>.BadResult("No se pudieron cargar las plantillas de memes.", new ListaPlantillasMeme());
			}
		}

		public async Task<Response<ResumenMemes>> GetResumenAsync(CancellationToken cancellationToken = default)
		{
			var limite = await LeerLimiteAsync(cancellationToken);
			var uso = await _memes.GetUsoHoy();
			return new Response<ResumenMemes>
			{
				IsSuccess = uso.IsSuccess,
				Message = uso.IsSuccess ? "Ok" : uso.Message,
				Data = new ResumenMemes { iUsoHoy = uso.IsSuccess ? uso.Data : 0, iLimiteDiario = limite }
			};
		}

		public async Task<Response<ResultadoMeme>> CrearAsync(int idUsuario, PostCrearMeme post, CancellationToken cancellationToken = default)
		{
			if (post == null || string.IsNullOrWhiteSpace(post.vchPlantillaId))
			{
				return Mala("Elige una plantilla de meme.");
			}

			var idea = MemePromptBuilder.LimpiarIdea(post.vchIdea);
			if (idea.Length < MemePromptBuilder.MinCaracteresIdea)
			{
				return Mala("Cuéntame la idea del meme (de qué trata o qué situación quieres mostrar).");
			}

			if (!await Simultaneas.WaitAsync(0, cancellationToken))
			{
				return Mala("Ya hay memes en creación. Espera a que terminen e inténtalo de nuevo.");
			}

			try
			{
				var limite = await LeerLimiteAsync(cancellationToken);
				var uso = await _memes.GetUsoHoy();
				if (!uso.IsSuccess)
				{
					return Mala("No se pudo validar el límite diario de memes. Inténtalo de nuevo.");
				}

				if (uso.Data >= limite)
				{
					return Mala($"Se alcanzó el límite de {limite} memes por hoy. Vuelve mañana o sube el valor del parámetro {ParametrosAlias.MemesLimiteDiario}.");
				}

				// La plantilla se resuelve por id en el servidor: nunca se acepta una URL que mande el cliente.
				var plantilla = await _plantillas.ObtenerAsync(post.vchPlantillaId, cancellationToken);
				if (plantilla == null)
				{
					return Mala("Esa plantilla ya no está disponible. Elige otra.");
				}

				// 1.ª pasada: reinterpretación suave. Si Gemini responde IMAGE_RECITATION (plantillas famosas
				// como DiCaprio/Drake), se reintenta una vez pidiendo caricatura original.
				var edicion = await _imagenes.EditarAsync(new PostEditarImagen
				{
					vchImagenUrl = plantilla.vchImagenUrl,
					vchPrompt = MemePromptBuilder.Componer(plantilla.nvchNombre, plantilla.iCajas, idea)
				}, cancellationToken);

				if ((!edicion.IsSuccess || string.IsNullOrWhiteSpace(edicion.Data)) && EsRecitacion(edicion.Message))
				{
					_logger.LogInformation("Gemini bloqueó la plantilla {Id} por IMAGE_RECITATION. Reintento estilizado.", plantilla.vchId);
					edicion = await _imagenes.EditarAsync(new PostEditarImagen
					{
						vchImagenUrl = plantilla.vchImagenUrl,
						vchPrompt = MemePromptBuilder.ComponerEstilizado(plantilla.nvchNombre, plantilla.iCajas, idea)
					}, cancellationToken);
				}

				if (!edicion.IsSuccess || string.IsNullOrWhiteSpace(edicion.Data))
				{
					var mensaje = string.IsNullOrWhiteSpace(edicion.Message) ? "No se pudo crear el meme." : edicion.Message;
					if (EsRecitacion(mensaje))
					{
						mensaje = "Gemini no pudo usar esta plantilla (es demasiado conocida). Elige otra plantilla o cambia la idea.";
					}
					return Mala(mensaje);
				}

				var meme = new MemeCreado
				{
					vchPlantillaId = plantilla.vchId,
					nvchPlantillaNombre = plantilla.nvchNombre,
					vchPlantillaUrl = plantilla.vchImagenUrl,
					nvchIdea = idea,
					vchResultadoUrl = edicion.Data,
					dtCreacion = DateTime.UtcNow
				};

				var guardado = await _memes.GuardarMeme(meme, idUsuario);
				if (guardado.IsSuccess)
				{
					meme.iIdMeme = guardado.Data;
				}
				else
				{
					// El meme ya existe en Blob: no se le niega al usuario, pero queda el aviso para revisar la tabla.
					_logger.LogWarning("El meme se creó pero no se pudo registrar en el historial: {Mensaje}", guardado.Message);
				}

				return new Response<ResultadoMeme>
				{
					IsSuccess = true,
					Message = "Meme creado con IA.",
					Data = new ResultadoMeme { Meme = meme, iUsoHoy = uso.Data + 1, iLimiteDiario = limite }
				};
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				return Mala("La IA tardó demasiado en responder. Inténtalo de nuevo.");
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error inesperado al crear un meme.");
				return Mala("No se pudo crear el meme. Inténtalo de nuevo.");
			}
			finally
			{
				Simultaneas.Release();
			}
		}

		private async Task<int> LeerLimiteAsync(CancellationToken cancellationToken)
		{
			var texto = await _parametros.GetValor1Async(ParametrosAlias.MemesLimiteDiario, cancellationToken);
			return int.TryParse(texto, out var limite) && limite > 0 ? limite : LimiteDiarioPorDefecto;
		}

		private static bool EsRecitacion(string? mensaje) =>
			!string.IsNullOrWhiteSpace(mensaje)
			&& (mensaje.Contains("IMAGE_RECITATION", StringComparison.OrdinalIgnoreCase)
				|| mensaje.Contains("RECITATION", StringComparison.OrdinalIgnoreCase));

		private static Response<ResultadoMeme> Mala(string mensaje) => Response<ResultadoMeme>.BadResult(mensaje, new ResultadoMeme());
	}
}

