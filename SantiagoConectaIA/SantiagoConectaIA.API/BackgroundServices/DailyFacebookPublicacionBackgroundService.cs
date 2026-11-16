using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.BackgroundServices
{
	/// <summary>Publica a diario en Facebook la noticia más reciente que todavía no se haya publicado.</summary>
	public class DailyFacebookPublicacionBackgroundService : PublicacionDiariaBackgroundService
	{
		public DailyFacebookPublicacionBackgroundService(
			IServiceProvider serviceProvider,
			IPublicacionAutomaticaService automatica,
			IRegistroPublicacionesService registro,
			PublicacionesBitacora bitacora,
			ILogger<DailyFacebookPublicacionBackgroundService> logger)
			: base(serviceProvider, automatica, registro, bitacora, logger)
		{
		}

		protected override string Nombre => "Facebook";

		protected override string ClaveBitacora => PublicacionesBitacora.Noticias;

		protected override string TipoRegistro => PublicacionesTipos.Noticia;

		protected override Task<TimeSpan> ObtenerHoraAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			automatica.HoraPublicacionAsync(cancellationToken);

		protected override async Task<bool> EstaActivaAsync(IPublicacionAutomaticaService automatica, CancellationToken cancellationToken) =>
			(await automatica.GetAsync(cancellationToken)).bActivo;

		protected override Task PublicarAsync(IServiceProvider servicios, CancellationToken cancellationToken) =>
			servicios.GetRequiredService<INoticiaAutomaticaService>().PublicarMasRecienteAsync(manual: false, cancellationToken);
	}
}
