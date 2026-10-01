using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public interface IGeminiImagenService
    {
        Task<Response<string>> EditarAsync(PostEditarImagen post, CancellationToken cancellationToken = default);
    }
}
