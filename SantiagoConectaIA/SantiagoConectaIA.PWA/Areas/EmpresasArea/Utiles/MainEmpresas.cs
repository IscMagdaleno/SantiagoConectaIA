using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles
{
    /// <summary>
    /// Esta clase actúa como el ViewModel para el Área de Empresas. 
    /// </summary>
    public class MainEmpresas
    {
        private string url = @"api/Empresas";

        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;

        // Propiedades de Estado Generales
        public List<Empresa> LstRegistros { get; set; } = new();
        public Empresa RegistroSeleccionado { get; set; } = new();
        public List<CatalogoEmpresa> LstCatalogos { get; set; } = new();

        // Propiedades de Estado para Pestañas
        public List<EmpresaUbicacion> LstUbicaciones { get; set; } = new();
        public List<EmpresaRedSocial> LstRedesSociales { get; set; } = new();
        public List<CategoriaCatalogo> LstCategorias { get; set; } = new();
        public List<ProductoServicio> LstProductos { get; set; } = new();

        public MainEmpresas(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;
        }

        public async Task<SeverityMessage> PostGetRegistros()
        {
            var APIUrl = url + "/PostGetEmpresas";
            var request = new PostGetEmpresas { bEstatus = true };
            var response = await _httpService.Post<PostGetEmpresas, Response<List<Empresa>>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRegistros = data);
            return validation;
        }

        public async Task<SeverityMessage> PostSaveRegistro()
        {
            var APIUrl = url + "/PostSaveEmpresa";
            var request = _mapper.Get<Empresa, PostSaveEmpresa>(RegistroSeleccionado);
            var response = await _httpService.Post<PostSaveEmpresa, Response<Empresa>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, 
                onSuccess: data => 
                {
                    RegistroSeleccionado = data;
                });
            return validation;
        }

        public async Task<SeverityMessage> PostGetCatalogos()
        {
            var APIUrl = url + "/PostGetCatalogoEmpresas";
            var response = await _httpService.Post<PostGetCatalogoEmpresa, Response<List<CatalogoEmpresa>>>(APIUrl, new PostGetCatalogoEmpresa());
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstCatalogos = data);
        }

        public async Task<SeverityMessage> PostGetUbicaciones()
        {
            var APIUrl = url + "/PostGetEmpresaUbicaciones";
            var response = await _httpService.Post<PostGetEmpresaUbicaciones, Response<List<EmpresaUbicacion>>>(APIUrl, new PostGetEmpresaUbicaciones { iIdEmpresa = RegistroSeleccionado.iIdEmpresa });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstUbicaciones = data);
        }

        public async Task<SeverityMessage> PostSaveUbicacion(EmpresaUbicacion item)
        {
            var APIUrl = url + "/PostSaveEmpresaUbicacion";
            var request = new PostSaveEmpresaUbicacion { EmpresaUbicacion = item };
            var response = await _httpService.Post<PostSaveEmpresaUbicacion, Response<EmpresaUbicacion>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostGetRedesSociales()
        {
            var APIUrl = url + "/PostGetEmpresaRedesSociales";
            var response = await _httpService.Post<PostGetEmpresaRedesSociales, Response<List<EmpresaRedSocial>>>(APIUrl, new PostGetEmpresaRedesSociales { iIdEmpresa = RegistroSeleccionado.iIdEmpresa });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRedesSociales = data);
        }

        public async Task<SeverityMessage> PostSaveRedSocial(EmpresaRedSocial item)
        {
            var APIUrl = url + "/PostSaveEmpresaRedSocial";
            var request = new PostSaveEmpresaRedSocial { EmpresaRedSocial = item };
            var response = await _httpService.Post<PostSaveEmpresaRedSocial, Response<EmpresaRedSocial>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostGetCategorias()
        {
            var APIUrl = url + "/PostGetCategoriasPorEmpresa";
            var response = await _httpService.Post<PostGetCategoriasPorEmpresa, Response<List<CategoriaCatalogo>>>(APIUrl, new PostGetCategoriasPorEmpresa { iIdEmpresa = RegistroSeleccionado.iIdEmpresa });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstCategorias = data);
        }

        public async Task<SeverityMessage> PostSaveCategoria(CategoriaCatalogo item)
        {
            var APIUrl = url + "/PostSaveCategoriaCatalogo";
            var request = new PostSaveCategoriaCatalogo { CategoriaCatalogo = item };
            var response = await _httpService.Post<PostSaveCategoriaCatalogo, Response<CategoriaCatalogo>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostGetProductos(int iIdCategoria)
        {
            var APIUrl = url + "/PostGetProductosPorCategoria";
            var response = await _httpService.Post<PostGetProductosPorCategoria, Response<List<ProductoServicio>>>(APIUrl, new PostGetProductosPorCategoria { iIdCategoriaCat = iIdCategoria });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstProductos = data);
        }

        public async Task<SeverityMessage> PostSaveProducto(ProductoServicio item, int iIdCategoria)
        {
            var APIUrl = url + "/PostSaveProductoServicio";
            var request = new PostSaveProductoServicio { ProductoServicio = item };
            var response = await _httpService.Post<PostSaveProductoServicio, Response<ProductoServicio>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }
    }
}
