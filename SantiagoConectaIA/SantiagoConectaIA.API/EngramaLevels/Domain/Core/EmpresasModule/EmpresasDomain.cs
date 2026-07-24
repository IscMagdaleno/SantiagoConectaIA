using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;

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
                var request = _mapperHelper.Get<EmpresaUbicacion, spSaveEmpresaUbicacion.Request>(postModel.EmpresaUbicacion);
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
                var request = _mapperHelper.Get<EmpresaRedSocial, spSaveEmpresaRedSocial.Request>(postModel.EmpresaRedSocial);
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
                var request = _mapperHelper.Get<CategoriaCatalogo, spSaveCategoriaCatalogo.Request>(postModel.CategoriaCatalogo);
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
                var request = _mapperHelper.Get<ProductoServicio, spSaveProductoServicio.Request>(postModel.ProductoServicio);
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

        public async Task<Response<ConfiguracionVisual>> GetConfiguracionVisual(PostGetConfiguracionVisual postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetConfiguracionVisual, spGetConfiguracionVisual.Request>(postModel);
                var result = await _empresasRepository.spGetConfiguracionVisual(request);
                return _responseHelper.Validacion<spGetConfiguracionVisual.Result, ConfiguracionVisual>(result);
            }
            catch (Exception ex) { return Response<ConfiguracionVisual>.BadResult(ex.Message, new ConfiguracionVisual()); }
        }

        public async Task<Response<ConfiguracionVisual>> SaveConfiguracionVisual(PostSaveConfiguracionVisual postModel)
        {
            try
            {
                var request = _mapperHelper.Get<ConfiguracionVisual, spSaveConfiguracionVisual.Request>(postModel.ConfiguracionVisual);
                var result = await _empresasRepository.spSaveConfiguracionVisual(request);
                var validation = _responseHelper.Validacion<spSaveConfiguracionVisual.Result, ConfiguracionVisual>(result);
                if (validation.IsSuccess)
                {
                    postModel.ConfiguracionVisual.iIdConfiguracion = validation.Data.iIdConfiguracion;
                    validation.Data = postModel.ConfiguracionVisual;
                }
                return validation;
            }
            catch (Exception ex) { return Response<ConfiguracionVisual>.BadResult(ex.Message, new ConfiguracionVisual()); }
        }

        public async Task<Response<Propietario>> SavePropietario(Propietario postModel)
        {
            try
            {
                var request = _mapperHelper.Get<Propietario, spSavePropietario.Request>(postModel);
                var result = await _empresasRepository.spSavePropietario(request);
                var validation = _responseHelper.Validacion<spSavePropietario.Result, Propietario>(result);
                if (validation.IsSuccess)
                {
                    postModel.iIdPropietario = validation.Data.iIdPropietario;
                    validation.Data = postModel;
                }
                return validation;
            }
            catch (Exception ex) { return Response<Propietario>.BadResult(ex.Message, new Propietario()); }
        }

        public async Task<Response<IEnumerable<Propietario>>> GetPropietario(PostGetPropietario postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetPropietario, spGetPropietario.Request>(postModel);
                var result = await _empresasRepository.spGetPropietario(request);
                return _responseHelper.Validacion<spGetPropietario.Result, Propietario>(result);
            }
            catch (Exception ex) { return Response<IEnumerable<Propietario>>.BadResult(ex.Message, new List<Propietario>()); }
        }

        public async Task<Response<Empresa>> SaveEmprendimientoFull(PostSaveEmprendimientoFull postModel)
        {
            try
            {
                var request = _mapperHelper.Get<Empresa, spSaveEmprendimientoCompleto.Request>(postModel.Empresa);
                
                // Set propietario Id from the Propietario object if it has been saved or exists
                request.iIdPropietario = postModel.Propietario.iIdPropietario > 0 ? postModel.Propietario.iIdPropietario : postModel.Empresa.iIdPropietario;
                
                var detalles = new {
                    RedesSociales = postModel.RedesSociales,
                    Ubicaciones = postModel.Ubicaciones,
                    Categorias = postModel.Categorias
                };
                
                request.vchJsonDetalles = System.Text.Json.JsonSerializer.Serialize(detalles);
                
                var result = await _empresasRepository.spSaveEmprendimientoCompleto(request);
                var validation = _responseHelper.Validacion<spSaveEmprendimientoCompleto.Result, Empresa>(result);
                
                if (validation.IsSuccess)
                {
                    postModel.Empresa.iIdEmpresa = validation.Data.iIdEmpresa;
                    validation.Data = postModel.Empresa;
                }
                return validation;
            }
            catch (Exception ex) { return Response<Empresa>.BadResult(ex.Message, new Empresa()); }
        }
    }
}
