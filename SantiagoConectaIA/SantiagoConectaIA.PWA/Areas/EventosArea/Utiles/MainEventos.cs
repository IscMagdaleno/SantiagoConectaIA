using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objetos.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using SantiagoConectaIA.Share.Objects.Common;
using Microsoft.AspNetCore.Components.Forms;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Utiles
{
    public class MainEventos
    {
        private string url = @"api/Eventos";

        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;

        public List<Evento> LstRegistros { get; set; } = new List<Evento>();
        public Evento RegistroSeleccionado { get; set; } = new Evento();
        public EventoDetalle DetalleSeleccionado { get; set; } = new EventoDetalle();
        public List<CategoriaEvento> LstCategorias { get; set; } = new();
        public List<ImagenRegistro> LstImagenes { get; set; } = new();
        public List<SucursalEvento> LstSucursales { get; set; } = new();

        public MainEventos(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;
        }

        public async Task<SeverityMessage> PostGetRegistros()
        {
            var APIUrl = url + "/PostGetEventos";
            var request = new PostGetEventos { bEstatus = true };
            var response = await _httpService.Post<PostGetEventos, Response<List<Evento>>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRegistros = data);
            return validation;
        }

        public async Task<SeverityMessage> PostGetDestacados()
        {
            var APIUrl = url + "/PostGetEventos";
            var request = new PostGetEventos { bDestacado = true, bEstatus = true };
            var response = await _httpService.Post<PostGetEventos, Response<List<Evento>>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRegistros = data);
            return validation;
        }

        public async Task<SeverityMessage> PostSaveRegistro()
        {
            var APIUrl = url + "/PostSaveEvento";
            var request = _mapper.Get<Evento, PostSaveEvento>(RegistroSeleccionado);
            var response = await _httpService.Post<PostSaveEvento, Response<Evento>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response,
                onSuccess: data =>
                {
                    RegistroSeleccionado = data;
                });
            return validation;
        }

        public async Task<SeverityMessage> PostGetDetalle(int iIdEvento)
        {
            var APIUrl = url + "/PostGetEventoDetalle";
            var response = await _httpService.Post<PostGetEventoDetalle, Response<EventoDetalle>>(APIUrl, new PostGetEventoDetalle { iIdEvento = iIdEvento });
            return _validaServicioService.ValidadionServicio(response, onSuccess: data =>
            {
                DetalleSeleccionado = data;
                LstImagenes = data?.imagenes ?? new List<ImagenRegistro>();
            });
        }

        public async Task<SeverityMessage> PostGetCategorias()
        {
            var APIUrl = url + "/PostGetCategoriaEventos";
            var response = await _httpService.Post<PostGetCategoriaEventos, Response<List<CategoriaEvento>>>(APIUrl, new PostGetCategoriaEventos());
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstCategorias = data);
        }

        public async Task<SeverityMessage> PostSaveCategoriaEvento(CategoriaEvento item)
        {
            var APIUrl = url + "/PostSaveCategoriaEvento";
            var request = _mapper.Get<CategoriaEvento, PostSaveCategoriaEvento>(item);
            var response = await _httpService.Post<PostSaveCategoriaEvento, Response<CategoriaEvento>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostSaveImagen(PostSaveImagenRegistro item)
        {
            var APIUrl = url + "/PostSaveImagenRegistro";
            var response = await _httpService.Post<PostSaveImagenRegistro, Response<ImagenRegistro>>(APIUrl, item);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostDeleteImagen(int iIdImagen)
        {
            var APIUrl = url + "/PostDeleteImagenRegistro";
            var response = await _httpService.Post<PostDeleteImagenRegistro, Response<ImagenRegistro>>(APIUrl, new PostDeleteImagenRegistro { iIdImagen = iIdImagen });
            return _validaServicioService.ValidadionServicio(response);
        }

		public async Task<Response<BlobSaved>> PostUploadImagen(IBrowserFile file)
		{
			var urlAzure = "api/AzureBlob/UploadImage-Eventos";

			var nombreUnico = $"{RegistroSeleccionado.vchNombre}_{Guid.NewGuid()}{Path.GetExtension(file.Name)}";

			using var memoryStream = new MemoryStream();
			await file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10).CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			using var imgContent = new StreamContent(memoryStream);

			var response = await _httpService.PostWithImage<Response<BlobSaved>>(
				urlAzure,
				imgContent,
				nombreUnico
			);

			return response.Response ?? Response<BlobSaved>.BadResult("Error al subir la imagen al servidor.", new BlobSaved());
		}

		public async Task<SeverityMessage> PostGetSucursales(int iIdEvento)
        {
            var APIUrl = url + "/PostGetEventosSucursales";
            var request = new PostGetEventosSucursales { iIdEvento = iIdEvento, bActivo = true };
            var response = await _httpService.Post<PostGetEventosSucursales, Response<List<SucursalEvento>>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response, onSuccess: data => LstSucursales = data);
        }

        public async Task<SeverityMessage> PostSaveSucursal()
        {
            var APIUrl = url + "/PostSaveSucursalEvento";
            var sucursalActual = LstSucursales.FirstOrDefault();
            if (sucursalActual == null) sucursalActual = new SucursalEvento();
            var request = _mapper.Get<SucursalEvento, PostSaveSucursalEvento>(sucursalActual);
            var response = await _httpService.Post<PostSaveSucursalEvento, Response<SucursalEvento>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }

        public async Task<SeverityMessage> PostSaveSucursalItem(SucursalEvento item)
        {
            var APIUrl = url + "/PostSaveSucursalEvento";
            var request = _mapper.Get<SucursalEvento, PostSaveSucursalEvento>(item);
            var response = await _httpService.Post<PostSaveSucursalEvento, Response<SucursalEvento>>(APIUrl, request);
            return _validaServicioService.ValidadionServicio(response);
        }
    }
}
