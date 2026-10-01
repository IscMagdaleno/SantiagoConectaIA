using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public interface IPublicacionAutomaticaService
    {
        Task<int> HoraPublicacionAsync(CancellationToken cancellationToken = default);
        Task<PublicacionAutomatica> GetAsync(CancellationToken cancellationToken = default);
        Task<PublicacionAutomatica> SetActivoAsync(bool activo, CancellationToken cancellationToken = default);
        Task RegistrarPublicacionAsync(int idNoticia, string titulo, CancellationToken cancellationToken = default);

        Task<int> HoraEmprendimientosAsync(CancellationToken cancellationToken = default);
        Task<PublicacionAutomaticaEmprendimientos> GetEmprendimientosAsync(CancellationToken cancellationToken = default);
        Task<PublicacionAutomaticaEmprendimientos> SetEmprendimientosActivoAsync(bool activo, CancellationToken cancellationToken = default);
        Task RegistrarEmprendimientoAsync(int idEmpresa, string nombre, CancellationToken cancellationToken = default);
    }
}
