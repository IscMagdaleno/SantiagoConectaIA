using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public interface IGeminiPublicacionService
    {
        Task<Response<string>> MejorarAsync(PostMejorarPublicacion post, CancellationToken cancellationToken = default);
        Task<Response<string>> MejorarEmprendimientoAsync(PostMejorarEmprendimiento post, CancellationToken cancellationToken = default);
        Task<Response<string>> MejorarProductoAsync(PostMejorarProducto post, CancellationToken cancellationToken = default);
        Task<Response<string>> MejorarEventoAsync(PostMejorarEvento post, CancellationToken cancellationToken = default);
    }
}
