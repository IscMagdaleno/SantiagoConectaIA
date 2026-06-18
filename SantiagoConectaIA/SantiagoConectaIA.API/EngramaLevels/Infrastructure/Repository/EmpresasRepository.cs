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
    }
}
