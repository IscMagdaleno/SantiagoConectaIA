using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>Publica a diario el siguiente emprendimiento de la rotación.</summary>
	public class DailyEmprendimientoPublicacionBackgroundService : PublicacionDiariaBackgroundService
	{
		public DailyEmprendimientoPublicacionBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			PublicacionesBitacora bitacora,
			ILogger<DailyEmprendimientoPublicacionBackgroundService> logger)
			: base(serviceProvider, automatica, bitacora, logger)
		{
		}

		protected override string Nombre => "emprendimientos";

		protected override string ClaveBitacora => PublicacionesBitacora.Emprendimientos;

		protected override Task<int> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			automatica.HoraEmprendimientosAsync(cancellationToken);

		protected override Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken) =>
			servicios.GetRequiredService<IEmprendimientoAutomaticoService>().PublicarSiguienteAsync(manual: false, cancellationToken);
	}
}
