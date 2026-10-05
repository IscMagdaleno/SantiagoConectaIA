using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.EventosModulo;

namespace SantiagoConectaIA.API.Services
{
    public interface IGeminiEventoService
    {
        Task<Response<EventoExtraidoDto>> ExtraerEventoDesdeImagenAsync(Stream imageStream, string mimeType, string imageUrl, CancellationToken cancellationToken = default);
    }
}
