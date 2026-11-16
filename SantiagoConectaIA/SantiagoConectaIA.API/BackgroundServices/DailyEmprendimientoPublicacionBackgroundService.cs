using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>Publica a diario el siguiente emprendimiento de la rotación.</summary>
	public class DailyEmprendimientoPublicacionBackgroundService : PublicacionDiariaBackgroundService
	{
		public DailyEmprendimientoPublicacionBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			IRegistroPublicacionesService registro,
			PublicacionesBitacora bitacora,
			ILogger<DailyEmprendimientoPublicacionBackgroundService> logger)
			: base(serviceProvider, automatica, registro, bitacora, logger)
		{
		}

		protected override string Nombre => "emprendimientos";

		protected override string ClaveBitacora => PublicacionesBitacora.Emprendimientos;

		protected override string TipoRegistro => PublicacionesTipos.Emprendimiento;

		protected override Task<TimeSpan> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			automatica.HoraEmprendimientosAsync(cancellationToken);

		protected override async Task<bool> EstaActivaAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			(await automatica.GetEmprendimientosAsync(cancellationToken)).bActivo;

		protected override Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken) =>
			servicios.GetRequiredService<IEmprendimientoAutomaticoService>().PublicarSiguienteAsync(manual: false, cancellationToken);
	}
}
