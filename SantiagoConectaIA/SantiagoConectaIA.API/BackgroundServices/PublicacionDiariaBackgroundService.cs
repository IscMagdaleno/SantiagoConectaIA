using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>
	/// Ciclo común de las publicaciones automáticas. Funciona igual que el scraper de noticias: no duerme hasta una
	/// hora exacta, sino que revisa al arrancar y cada pocos minutos. Publica cuando ya pasó la hora configurada
	/// (hora de México) y todavía no hay una publicación exitosa hoy en <c>SCIA.PublicacionesRegistro</c>.
	/// Así, si el servidor estaba dormido o reiniciado a esa hora, publica en cuanto despierta ese mismo día,
	/// y nunca publica dos veces el mismo día aunque se reinicie.
	/// </summary>
	public abstract class PublicacionDiariaBackgroundService : BackgroundService
	{
		/// <summary>Cada cuánto se revisa si toca publicar.</summary>
		private static readonly TimeSpan IntervaloRevision = TimeSpan.FromMinutes(10);

		/// <summary>Espera al arrancar para dejar que la aplicación termine de iniciar antes de la primera revisión.</summary>
		private static readonly TimeSpan EsperaInicial = TimeSpan.FromSeconds(20);

		/// <summary>Tiempo mínimo entre un intento fallido y el siguiente.</summary>
		private static readonly TimeSpan EsperaEntreReintentos = TimeSpan.FromMinutes(15);

		/// <summary>Intentos automáticos fallidos permitidos por día antes de rendirse hasta mañana.</summary>
		private const int MaxIntentosFallidosPorDia = 3;

		private readonly IServiceProvider _serviceProvider;
		private readonly IPublicacionAutomaticaService _automatica;
		private readonly IRegistroPublicacionesService _registro;
		private readonly PublicacionesBitacora _bitacora;
		private readonly ILogger _logger;

		/// <summary>
		/// No se vuelve a intentar antes de esta hora. Cubre los casos en que no hubo qué publicar (por ejemplo,
		/// todavía no llega una noticia nueva) y que por eso no quedan en el registro como intento fallido.
		/// </summary>
		private DateTime _noAntesDeUtc = DateTime.MinValue;

		protected PublicacionDiariaBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			IRegistroPublicacionesService registro,
			PublicacionesBitacora bitacora,
			ILogger logger)
		{
			_serviceProvider = serviceProvider;
			_automatica = automatica;
			_registro = registro;
			_bitacora = bitacora;
			_logger = logger;
		}

		/// <summary>Nombre para los mensajes de log (por ejemplo "Facebook" o "emprendimientos").</summary>
		protected abstract string Nombre { get; }

		/// <summary>Clave de <see cref="PublicacionesBitacora"/> donde se registra esta publicación.</summary>
		protected abstract string ClaveBitacora { get; }

		/// <summary>Tipo en el registro de publicaciones (<see cref="PublicacionesTipos"/>).</summary>
		protected abstract string TipoRegistro { get; }

		/// <summary>Hora del día (hora de México) en que se publica.</summary>
		protected abstract Task<TimeSpan> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken);

		/// <summary>Si el interruptor de la publicación diaria está encendido.</summary>
		protected abstract Task<bool> EstaActivaAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken);

		/// <summary>Publica lo que toque. Se ejecuta dentro de un scope propio.</summary>
		protected abstract Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Publicación automática de {Nombre} iniciada. Se revisa cada {Minutos} minutos.", Nombre, (int)IntervaloRevision.TotalMinutes);

			try
			{
				await Task.Delay(EsperaInicial, stoppingToken);

				while (!stoppingToken.IsCancellationRequested)
				{
					var espera = IntervaloRevision;
					try
					{
						espera = await RevisarAsync(stoppingToken);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						break;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error en la publicación automática de {Nombre}.", Nombre);
						_bitacora.Registrar(ClaveBitacora, $"Error: {ex.Message}");
					}

					await Task.Delay(espera, stoppingToken);
				}
			}
			catch (OperationCanceledException)
			{
				// El servidor se está apagando.
			}
		}

		/// <summary>Una revisión: decide si toca publicar y, si es así, publica. Devuelve cuánto esperar para la siguiente.</summary>
		private async Task<TimeSpan> RevisarAsync(CancellationToken cancellationToken)
		{
			var hora = await ObtenerHoraAsync(_automatica, cancellationToken);
			var proxima = HorarioPublicacion.ProximaUtc(hora);

			if (!await EstaActivaAsync(_automatica, cancellationToken))
			{
				_bitacora.Programar(ClaveBitacora, proxima);
				return IntervaloRevision;
			}

			// Todavía no es la hora de hoy: se espera sin tocar la base de datos.
			if (!HorarioPublicacion.YaPasoLaHora(hora))
			{
				_bitacora.Programar(ClaveBitacora, proxima);
				var faltante = HorarioPublicacion.TiempoHasta(hora);
				return faltante < IntervaloRevision ? faltante + TimeSpan.FromSeconds(5) : IntervaloRevision;
			}

			if (DateTime.UtcNow < _noAntesDeUtc)
			{
				_bitacora.Programar(ClaveBitacora, _noAntesDeUtc);
				return IntervaloRevision;
			}

			var hoy = await _registro.ResumenHoyAsync(TipoRegistro, cancellationToken);
			if (hoy.Exitosas > 0)
			{
				_bitacora.Programar(ClaveBitacora, proxima);
				return IntervaloRevision;
			}

			if (hoy.FallidasAutomaticas >= MaxIntentosFallidosPorDia)
			{
				_logger.LogWarning("La publicación automática de {Nombre} falló {Intentos} veces hoy. Se reintenta mañana.", Nombre, hoy.FallidasAutomaticas);
				_bitacora.Programar(ClaveBitacora, proxima);
				return IntervaloRevision;
			}

			if (hoy.UltimoIntentoAutomaticoUtc.HasValue && DateTime.UtcNow - hoy.UltimoIntentoAutomaticoUtc.Value < EsperaEntreReintentos)
			{
				_bitacora.Programar(ClaveBitacora, hoy.UltimoIntentoAutomaticoUtc.Value + EsperaEntreReintentos);
				return IntervaloRevision;
			}

			_logger.LogInformation("Es hora de la publicación automática de {Nombre} ({Hora}) y hoy no se ha publicado.", Nombre, HorarioPublicacion.Formato(hora));
			_bitacora.Programar(ClaveBitacora, DateTime.UtcNow);

			_noAntesDeUtc = DateTime.UtcNow + EsperaEntreReintentos;

			using var scope = _serviceProvider.CreateScope();
			await PublicarAsync(scope.ServiceProvider, cancellationToken);
			return IntervaloRevision;
		}
	}
}
