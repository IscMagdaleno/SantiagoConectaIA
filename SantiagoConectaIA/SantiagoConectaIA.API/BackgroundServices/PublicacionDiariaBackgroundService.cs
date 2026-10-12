using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>
	/// Ciclo común de las publicaciones automáticas: espera hasta la hora configurada (hora de México),
	/// publica dentro de un scope nuevo y deja el resultado en la bitácora. Cada publicación solo define
	/// qué publicar y a qué hora.
	/// </summary>
	public abstract class PublicacionDiariaBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IPublicacionAutomaticaService _automatica;
		private readonly PublicacionesBitacora _bitacora;
		private readonly ILogger _logger;

		protected PublicacionDiariaBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			PublicacionesBitacora bitacora,
			ILogger logger)
		{
			_serviceProvider = serviceProvider;
			_automatica = automatica;
			_bitacora = bitacora;
			_logger = logger;
		}

		/// <summary>Nombre para los mensajes de log (por ejemplo "Facebook" o "emprendimientos").</summary>
		protected abstract string Nombre { get; }

		/// <summary>Clave de <see cref="PublicacionesBitacora"/> donde se registra esta publicación.</summary>
		protected abstract string ClaveBitacora { get; }

		/// <summary>Hora del día (0-23, hora de México) en que se publica.</summary>
		protected abstract Task<int> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken);

		/// <summary>Publica lo que toque. Se ejecuta dentro de un scope propio.</summary>
		protected abstract Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Publicación automática de {Nombre} iniciada.", Nombre);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var hora = await ObtenerHoraAsync(_automatica, stoppingToken);
					var espera = HorarioPublicacion.TiempoHasta(hora);
					_logger.LogInformation("Siguiente publicación automática de {Nombre} a las {Hora}:00, en {Espera}.", Nombre, hora, espera);
					_bitacora.Programar(ClaveBitacora, DateTime.UtcNow.Add(espera));
					await Task.Delay(espera, stoppingToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}

				try
				{
					using var scope = _serviceProvider.CreateScope();
					await PublicarAsync(scope.ServiceProvider, stoppingToken);
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
			}
		}
	}
}
