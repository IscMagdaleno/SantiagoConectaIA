using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class EmpresasRepository : IEmpresasRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public EmpresasRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetEmpresas.Result>> spGetEmpresas(spGetEmpresas.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEmpresas.Result, spGetEmpresas.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetEmpresas.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveEmpresa.Result> spSaveEmpresa(spSaveEmpresa.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveEmpresa.Result, spSaveEmpresa.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveEmpresa.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetCatalogoEmpresa.Result>> spGetCatalogoEmpresa(spGetCatalogoEmpresa.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetCatalogoEmpresa.Result, spGetCatalogoEmpresa.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetCatalogoEmpresa.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetEmpresaUbicaciones.Result>> spGetEmpresaUbicaciones(spGetEmpresaUbicaciones.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEmpresaUbicaciones.Result, spGetEmpresaUbicaciones.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetEmpresaUbicaciones.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveEmpresaUbicacion.Result> spSaveEmpresaUbicacion(spSaveEmpresaUbicacion.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveEmpresaUbicacion.Result, spSaveEmpresaUbicacion.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveEmpresaUbicacion.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetEmpresaRedesSociales.Result>> spGetEmpresaRedesSociales(spGetEmpresaRedesSociales.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEmpresaRedesSociales.Result, spGetEmpresaRedesSociales.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetEmpresaRedesSociales.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveEmpresaRedSocial.Result> spSaveEmpresaRedSocial(spSaveEmpresaRedSocial.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveEmpresaRedSocial.Result, spSaveEmpresaRedSocial.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveEmpresaRedSocial.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetCategoriasPorEmpresa.Result>> spGetCategoriasPorEmpresa(spGetCategoriasPorEmpresa.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetCategoriasPorEmpresa.Result, spGetCategoriasPorEmpresa.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetCategoriasPorEmpresa.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveCategoriaCatalogo.Result> spSaveCategoriaCatalogo(spSaveCategoriaCatalogo.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveCategoriaCatalogo.Result, spSaveCategoriaCatalogo.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveCategoriaCatalogo.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetProductosPorCategoria.Result>> spGetProductosPorCategoria(spGetProductosPorCategoria.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetProductosPorCategoria.Result, spGetProductosPorCategoria.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetProductosPorCategoria.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveProductoServicio.Result> spSaveProductoServicio(spSaveProductoServicio.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveProductoServicio.Result, spSaveProductoServicio.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveProductoServicio.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spGetConfiguracionVisual.Result> spGetConfiguracionVisual(spGetConfiguracionVisual.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spGetConfiguracionVisual.Result, spGetConfiguracionVisual.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spGetConfiguracionVisual.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveConfiguracionVisual.Result> spSaveConfiguracionVisual(spSaveConfiguracionVisual.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveConfiguracionVisual.Result, spSaveConfiguracionVisual.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new spSaveConfiguracionVisual.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
