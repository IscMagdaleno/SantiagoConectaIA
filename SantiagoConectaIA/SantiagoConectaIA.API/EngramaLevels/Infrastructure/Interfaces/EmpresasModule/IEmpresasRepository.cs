using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule
{
    public interface IEmpresasRepository
    {
        Task<IEnumerable<spGetEmpresas.Result>> spGetEmpresas(spGetEmpresas.Request request);
        Task<spSaveEmpresa.Result> spSaveEmpresa(spSaveEmpresa.Request request);
    }
}
