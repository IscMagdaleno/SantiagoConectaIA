using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EventosModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class EventosRepository : IEventosRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public EventosRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetEventos.Result>> spGetEventos(spGetEventos.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEventos.Result, spGetEventos.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetEventos.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveEvento.Result> spSaveEvento(spSaveEvento.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveEvento.Result, spSaveEvento.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveEvento.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetCategoriaEventos.Result>> spGetCategoriaEventos(spGetCategoriaEventos.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetCategoriaEventos.Result, spGetCategoriaEventos.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetCategoriaEventos.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveCategoriaEvento.Result> spSaveCategoriaEvento(spSaveCategoriaEvento.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveCategoriaEvento.Result, spSaveCategoriaEvento.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveCategoriaEvento.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetImagenesRegistro.Result>> spGetImagenesRegistro(spGetImagenesRegistro.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetImagenesRegistro.Result, spGetImagenesRegistro.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetImagenesRegistro.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveImagenRegistro.Result> spSaveImagenRegistro(spSaveImagenRegistro.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveImagenRegistro.Result, spSaveImagenRegistro.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveImagenRegistro.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spDeleteImagenRegistro.Result> spDeleteImagenRegistro(spDeleteImagenRegistro.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spDeleteImagenRegistro.Result, spDeleteImagenRegistro.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spDeleteImagenRegistro.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetEventoDetalle.Result>> spGetEventoDetalle(spGetEventoDetalle.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEventoDetalle.Result, spGetEventoDetalle.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetEventoDetalle.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetEventosSucursales.Result>> spGetEventosSucursales(spGetEventosSucursales.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEventosSucursales.Result, spGetEventosSucursales.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetEventosSucursales.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveSucursalEvento.Result> spSaveSucursalEvento(spSaveSucursalEvento.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveSucursalEvento.Result, spSaveSucursalEvento.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveSucursalEvento.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
