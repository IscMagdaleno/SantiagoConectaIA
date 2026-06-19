using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core.EmpresasModule
{
    public class EmpresasDomain : IEmpresasDomain
    {
        private readonly IEmpresasRepository _empresasRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public EmpresasDomain(IEmpresasRepository empresasRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _empresasRepository = empresasRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<Empresa>>> GetEmpresas(PostGetEmpresas postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEmpresas, spGetEmpresas.Request>(postModel);
                var result = await _empresasRepository.spGetEmpresas(request);
                return _responseHelper.Validacion<spGetEmpresas.Result, Empresa>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Empresa>>.BadResult(ex.Message, new List<Empresa>());
            }
        }

        public async Task<Response<Empresa>> SaveEmpresa(PostSaveEmpresa postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveEmpresa, spSaveEmpresa.Request>(postModel);
                var result = await _empresasRepository.spSaveEmpresa(request);
                var validation = _responseHelper.Validacion<spSaveEmpresa.Result, Empresa>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdEmpresa = validation.Data.iIdEmpresa;
                    validation.Data = _mapperHelper.Get<PostSaveEmpresa, Empresa>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<Empresa>.BadResult(ex.Message, new Empresa());
            }
        }

        public async Task<Response<IEnumerable<CatalogoEmpresa>>> GetCatalogoEmpresas(PostGetCatalogoEmpresa postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetCatalogoEmpresa, spGetCatalogoEmpresa.Request>(postModel);
                var result = await _empresasRepository.spGetCatalogoEmpresa(request);
                return _responseHelper.Validacion<spGetCatalogoEmpresa.Result, CatalogoEmpresa>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<CatalogoEmpresa>>.BadResult(ex.Message, new List<CatalogoEmpresa>()); }
        }

        public async Task<Response<IEnumerable<EmpresaUbicacion>>> GetEmpresaUbicaciones(PostGetEmpresaUbicaciones postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEmpresaUbicaciones, spGetEmpresaUbicaciones.Request>(postModel);
                var result = await _empresasRepository.spGetEmpresaUbicaciones(request);
                return _responseHelper.Validacion<spGetEmpresaUbicaciones.Result, EmpresaUbicacion>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<EmpresaUbicacion>>.BadResult(ex.Message, new List<EmpresaUbicacion>()); }
        }

        public async Task<Response<EmpresaUbicacion>> SaveEmpresaUbicacion(PostSaveEmpresaUbicacion postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveEmpresaUbicacion, spSaveEmpresaUbicacion.Request>(postModel);
                var result = await _empresasRepository.spSaveEmpresaUbicacion(request);
                var validation = _responseHelper.Validacion<spSaveEmpresaUbicacion.Result, EmpresaUbicacion>(result);
                if (validation.IsSuccess)
                {
                    postModel.EmpresaUbicacion.iIdUbicacion = validation.Data.iIdUbicacion;
                    validation.Data = postModel.EmpresaUbicacion;
                }
                return validation;
            }
            catch (Exception ex) { return Response<EmpresaUbicacion>.BadResult(ex.Message, new EmpresaUbicacion()); }
        }

        public async Task<Response<IEnumerable<EmpresaRedSocial>>> GetEmpresaRedesSociales(PostGetEmpresaRedesSociales postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEmpresaRedesSociales, spGetEmpresaRedesSociales.Request>(postModel);
                var result = await _empresasRepository.spGetEmpresaRedesSociales(request);
                return _responseHelper.Validacion<spGetEmpresaRedesSociales.Result, EmpresaRedSocial>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<EmpresaRedSocial>>.BadResult(ex.Message, new List<EmpresaRedSocial>()); }
        }

        public async Task<Response<EmpresaRedSocial>> SaveEmpresaRedSocial(PostSaveEmpresaRedSocial postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveEmpresaRedSocial, spSaveEmpresaRedSocial.Request>(postModel);
                var result = await _empresasRepository.spSaveEmpresaRedSocial(request);
                var validation = _responseHelper.Validacion<spSaveEmpresaRedSocial.Result, EmpresaRedSocial>(result);
                if (validation.IsSuccess)
                {
                    postModel.EmpresaRedSocial.iIdRedSocial = validation.Data.iIdRedSocial;
                    validation.Data = postModel.EmpresaRedSocial;
                }
                return validation;
            }
            catch (Exception ex) { return Response<EmpresaRedSocial>.BadResult(ex.Message, new EmpresaRedSocial()); }
        }

        public async Task<Response<IEnumerable<CategoriaCatalogo>>> GetCategoriasPorEmpresa(PostGetCategoriasPorEmpresa postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetCategoriasPorEmpresa, spGetCategoriasPorEmpresa.Request>(postModel);
                var result = await _empresasRepository.spGetCategoriasPorEmpresa(request);
                return _responseHelper.Validacion<spGetCategoriasPorEmpresa.Result, CategoriaCatalogo>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<CategoriaCatalogo>>.BadResult(ex.Message, new List<CategoriaCatalogo>()); }
        }

        public async Task<Response<CategoriaCatalogo>> SaveCategoriaCatalogo(PostSaveCategoriaCatalogo postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveCategoriaCatalogo, spSaveCategoriaCatalogo.Request>(postModel);
                var result = await _empresasRepository.spSaveCategoriaCatalogo(request);
                var validation = _responseHelper.Validacion<spSaveCategoriaCatalogo.Result, CategoriaCatalogo>(result);
                if (validation.IsSuccess)
                {
                    postModel.CategoriaCatalogo.iIdCategoriaCat = validation.Data.iIdCategoriaCat;
                    validation.Data = postModel.CategoriaCatalogo;
                }
                return validation;
            }
            catch (Exception ex) { return Response<CategoriaCatalogo>.BadResult(ex.Message, new CategoriaCatalogo()); }
        }

        public async Task<Response<IEnumerable<ProductoServicio>>> GetProductosPorCategoria(PostGetProductosPorCategoria postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetProductosPorCategoria, spGetProductosPorCategoria.Request>(postModel);
                var result = await _empresasRepository.spGetProductosPorCategoria(request);
                return _responseHelper.Validacion<spGetProductosPorCategoria.Result, ProductoServicio>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<ProductoServicio>>.BadResult(ex.Message, new List<ProductoServicio>()); }
        }

        public async Task<Response<ProductoServicio>> SaveProductoServicio(PostSaveProductoServicio postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveProductoServicio, spSaveProductoServicio.Request>(postModel);
                var result = await _empresasRepository.spSaveProductoServicio(request);
                var validation = _responseHelper.Validacion<spSaveProductoServicio.Result, ProductoServicio>(result);
                if (validation.IsSuccess)
                {
                    postModel.ProductoServicio.iIdProducto = validation.Data.iIdProducto;
                    validation.Data = postModel.ProductoServicio;
                }
                return validation;
            }
            catch (Exception ex) { return Response<ProductoServicio>.BadResult(ex.Message, new ProductoServicio()); }
        }
    }
}
