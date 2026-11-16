using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Configuración de las publicaciones automáticas: hora (parámetros), interruptor de activación y
	/// estado para el PWA (qué se publicó por última vez, que sale del registro en base de datos).
	/// </summary>
	public interface IPublicacionAutomaticaService
	{
		Task<TimeSpan> HoraPublicacionAsync(CancellationToken cancellationToken = default);
		Task<PublicacionAutomatica> GetAsync(CancellationToken cancellationToken = default);
		Task<PublicacionAutomatica> SetActivoAsync(bool activo, CancellationToken cancellationToken = default);

		Task<TimeSpan> HoraEmprendimientosAsync(CancellationToken cancellationToken = default);
		Task<PublicacionAutomaticaEmprendimientos> GetEmprendimientosAsync(CancellationToken cancellationToken = default);
		Task<PublicacionAutomaticaEmprendimientos> SetEmprendimientosActivoAsync(bool activo, CancellationToken cancellationToken = default);
	}
}
