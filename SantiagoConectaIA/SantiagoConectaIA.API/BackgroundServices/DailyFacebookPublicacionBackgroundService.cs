using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>Publica a diario la noticia más reciente en Facebook.</summary>
	public class DailyFacebookPublicacionBackgroundService : PublicacionDiariaBackgroundService
	{
		public DailyFacebookPublicacionBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			PublicacionesBitacora bitacora,
			ILogger<DailyFacebookPublicacionBackgroundService> logger)
			: base(serviceProvider, automatica, bitacora, logger)
		{
		}

		protected override string Nombre => "Facebook";

		protected override string ClaveBitacora => PublicacionesBitacora.Noticias;

		protected override Task<int> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			automatica.HoraPublicacionAsync(cancellationToken);

		protected override Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken) =>
			servicios.GetRequiredService<INoticiaAutomaticaService>().PublicarMasRecienteAsync(manual: false, cancellationToken);
	}
}
