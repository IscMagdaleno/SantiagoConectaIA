using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule
{
    public interface IEmpresasDomain
    {
        Task<Response<IEnumerable<Empresa>>> GetEmpresas(PostGetEmpresas postModel);
        Task<Response<Empresa>> SaveEmpresa(PostSaveEmpresa postModel);
    }
}
