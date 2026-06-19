using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule
{
    public interface IEmpresasRepository
    {
        Task<IEnumerable<spGetEmpresas.Result>> spGetEmpresas(spGetEmpresas.Request request);
        Task<spSaveEmpresa.Result> spSaveEmpresa(spSaveEmpresa.Request request);
        Task<IEnumerable<spGetCatalogoEmpresa.Result>> spGetCatalogoEmpresa(spGetCatalogoEmpresa.Request request);
        Task<IEnumerable<spGetEmpresaUbicaciones.Result>> spGetEmpresaUbicaciones(spGetEmpresaUbicaciones.Request request);
        Task<spSaveEmpresaUbicacion.Result> spSaveEmpresaUbicacion(spSaveEmpresaUbicacion.Request request);
        Task<IEnumerable<spGetEmpresaRedesSociales.Result>> spGetEmpresaRedesSociales(spGetEmpresaRedesSociales.Request request);
        Task<spSaveEmpresaRedSocial.Result> spSaveEmpresaRedSocial(spSaveEmpresaRedSocial.Request request);
        Task<IEnumerable<spGetCategoriasPorEmpresa.Result>> spGetCategoriasPorEmpresa(spGetCategoriasPorEmpresa.Request request);
        Task<spSaveCategoriaCatalogo.Result> spSaveCategoriaCatalogo(spSaveCategoriaCatalogo.Request request);
        Task<IEnumerable<spGetProductosPorCategoria.Result>> spGetProductosPorCategoria(spGetProductosPorCategoria.Request request);
        Task<spSaveProductoServicio.Result> spSaveProductoServicio(spSaveProductoServicio.Request request);
    }
}
