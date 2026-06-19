using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using SantiagoConectaIA.Share.Objects.Common;
using Microsoft.AspNetCore.Components.Forms;

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
        public List<Empresa> LstRegistros { get; set; } = new List<Empresa>();
        public Empresa RegistroSeleccionado { get; set; } = new Empresa();
        public ConfiguracionVisual ConfiguracionSeleccionada { get; set; } = new ConfiguracionVisual();
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

        public async Task<Response<BlobSaved>> PostUploadLogo(IBrowserFile file)
        {
            // Apuntar al endpoint específico de UploadImage que creamos en AzureBlobController
            var urlAzure = "api/AzureBlob/UploadImage"; 
            
            using var memoryStream = new MemoryStream();
            await file.OpenReadStream(1024 * 1024 * 10).CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            StreamContent imgContent = new StreamContent(memoryStream);

            var response = await _httpService.PostWithFile<Response<BlobSaved>>(urlAzure, imgContent);
            return response.Response;
        }

        public async Task<SeverityMessage> PostGetConfiguracionVisual()
        {
            var APIUrl = url + "/PostGetConfiguracionVisual";
            var response = await _httpService.Post<PostGetConfiguracionVisual, Response<ConfiguracionVisual>>(APIUrl, new PostGetConfiguracionVisual { iIdEmpresa = RegistroSeleccionado.iIdEmpresa });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => ConfiguracionSeleccionada = data ?? new ConfiguracionVisual { iIdEmpresa = RegistroSeleccionado.iIdEmpresa });
        }

        public async Task<SeverityMessage> PostSaveConfiguracionVisual()
        {
            var APIUrl = url + "/PostSaveConfiguracionVisual";
            ConfiguracionSeleccionada.iIdEmpresa = RegistroSeleccionado.iIdEmpresa;
            var request = new PostSaveConfiguracionVisual { ConfiguracionVisual = ConfiguracionSeleccionada };
            var response = await _httpService.Post<PostSaveConfiguracionVisual, Response<ConfiguracionVisual>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => ConfiguracionSeleccionada = data);
        }
    }
}
