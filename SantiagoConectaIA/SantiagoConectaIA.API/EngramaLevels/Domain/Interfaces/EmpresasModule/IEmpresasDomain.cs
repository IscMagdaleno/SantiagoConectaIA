using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule
{
    public interface IEmpresasDomain
    {
        Task<Response<IEnumerable<Empresa>>> GetEmpresas(PostGetEmpresas postModel);
        Task<Response<Empresa>> SaveEmpresa(PostSaveEmpresa postModel);
        Task<Response<IEnumerable<CatalogoEmpresa>>> GetCatalogoEmpresas(PostGetCatalogoEmpresa postModel);
        Task<Response<IEnumerable<EmpresaUbicacion>>> GetEmpresaUbicaciones(PostGetEmpresaUbicaciones postModel);
        Task<Response<EmpresaUbicacion>> SaveEmpresaUbicacion(PostSaveEmpresaUbicacion postModel);
        Task<Response<IEnumerable<EmpresaRedSocial>>> GetEmpresaRedesSociales(PostGetEmpresaRedesSociales postModel);
        Task<Response<EmpresaRedSocial>> SaveEmpresaRedSocial(PostSaveEmpresaRedSocial postModel);
        Task<Response<IEnumerable<CategoriaCatalogo>>> GetCategoriasPorEmpresa(PostGetCategoriasPorEmpresa postModel);
        Task<Response<CategoriaCatalogo>> SaveCategoriaCatalogo(PostSaveCategoriaCatalogo postModel);
        Task<Response<IEnumerable<ProductoServicio>>> GetProductosPorCategoria(PostGetProductosPorCategoria postModel);
        Task<Response<ProductoServicio>> SaveProductoServicio(PostSaveProductoServicio postModel);
        Task<Response<ConfiguracionVisual>> GetConfiguracionVisual(PostGetConfiguracionVisual postModel);
        Task<Response<ConfiguracionVisual>> SaveConfiguracionVisual(PostSaveConfiguracionVisual postModel);
        Task<Response<Propietario>> SavePropietario(Propietario postModel);
        Task<Response<IEnumerable<Propietario>>> GetPropietario(PostGetPropietario postModel);
        Task<Response<Empresa>> SaveEmprendimientoFull(PostSaveEmprendimientoFull postModel);
    }
}
