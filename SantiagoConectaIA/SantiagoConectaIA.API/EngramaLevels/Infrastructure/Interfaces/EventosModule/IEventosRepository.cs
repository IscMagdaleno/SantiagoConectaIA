using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EventosModule
{
    public interface IEventosRepository
    {
        Task<IEnumerable<spGetEventos.Result>> spGetEventos(spGetEventos.Request request);
        Task<spSaveEvento.Result> spSaveEvento(spSaveEvento.Request request);
        Task<IEnumerable<spGetCategoriaEventos.Result>> spGetCategoriaEventos(spGetCategoriaEventos.Request request);
        Task<spSaveCategoriaEvento.Result> spSaveCategoriaEvento(spSaveCategoriaEvento.Request request);
        Task<IEnumerable<spGetImagenesRegistro.Result>> spGetImagenesRegistro(spGetImagenesRegistro.Request request);
        Task<spSaveImagenRegistro.Result> spSaveImagenRegistro(spSaveImagenRegistro.Request request);
        Task<spDeleteImagenRegistro.Result> spDeleteImagenRegistro(spDeleteImagenRegistro.Request request);
        Task<IEnumerable<spGetEventoDetalle.Result>> spGetEventoDetalle(spGetEventoDetalle.Request request);
        Task<IEnumerable<spGetEventosSucursales.Result>> spGetEventosSucursales(spGetEventosSucursales.Request request);
        Task<spSaveSucursalEvento.Result> spSaveSucursalEvento(spSaveSucursalEvento.Request request);
    }
}
