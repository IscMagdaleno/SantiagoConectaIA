using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public interface IFacebookPublishService
    {
        Task<Response<string>> PublicarAsync(PostPublicarFacebook post, CancellationToken cancellationToken = default);
    }
}
