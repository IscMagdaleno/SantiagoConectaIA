using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule
{
    public interface IEventosDomain
    {
        Task<Response<IEnumerable<Evento>>> GetEventos(PostGetEventos postModel);
        Task<Response<Evento>> SaveEvento(PostSaveEvento postModel);
        Task<Response<IEnumerable<CategoriaEvento>>> GetCategoriaEventos(PostGetCategoriaEventos postModel);
        Task<Response<CategoriaEvento>> SaveCategoriaEvento(PostSaveCategoriaEvento postModel);
        Task<Response<IEnumerable<ImagenRegistro>>> GetImagenesRegistro(PostGetImagenesRegistro postModel);
        Task<Response<ImagenRegistro>> SaveImagenRegistro(PostSaveImagenRegistro postModel);
        Task<Response<ImagenRegistro>> DeleteImagenRegistro(PostDeleteImagenRegistro postModel);
        Task<Response<EventoDetalle>> GetEventoDetalle(PostGetEventoDetalle postModel);
        Task<Response<IEnumerable<SucursalEvento>>> GetEventosSucursales(PostGetEventosSucursales postModel);
        Task<Response<SucursalEvento>> SaveSucursalEvento(PostSaveSucursalEvento postModel);
    }
}
