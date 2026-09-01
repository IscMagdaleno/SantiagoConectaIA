using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components.Forms;
using SantiagoConectaIA.Share.Objects.Common;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles
{
    public class MainNoticias
    {
        private string url = @"api/Noticias";
        private string urlAzure = @"api/AzureBlob/UploadDocument";

        #region INJECTS
        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;
        #endregion

        #region PROPIEDADES
        public List<Noticia> LstNoticias { get; set; }
        public List<TipoDatoDto> LstTipoDatos { get; set; }
        public Noticia NoticiaSelected { get; set; }
        public IBrowserFile SelectedImage { get; set; }
        #endregion

        public MainNoticias(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;

            LstNoticias = new List<Noticia>();
            LstTipoDatos = new List<TipoDatoDto>();
            NoticiaSelected = new Noticia();
        }

        public async Task<SeverityMessage> PostGetTipoDatos()
        {
            var APIUrl = "api/Catalogos/PostGetTipoDatos";
            var response = await _httpService.Post<object, Response<List<TipoDatoDto>>>(APIUrl, new { });
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstTipoDatos = data.ToList());
            return validation;
        }

        public async Task<SeverityMessage> PostGetNoticias()
        {
            var APIUrl = url + "/PostGetNoticias";
            var model = new PostGetNoticias { bActivo = true };
            var response = await _httpService.Post<PostGetNoticias, Response<List<Noticia>>>(APIUrl, model);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstNoticias = data.ToList());
            return validation;
        }

        public async Task<SeverityMessage> PostSaveNoticia(Noticia? noticia = null)
        {
            var target = noticia ?? NoticiaSelected;
            var APIUrl = url + "/PostSaveNoticia";
            // Map target to PostSaveNoticia
            var model = _mapper.Get<Noticia, PostSaveNoticia>(target);
            
            // Ensure child lists, cover and categories are included
            model.vchImagenPortada = target.vchImagenPortada;
            model.Imagenes = target.Imagenes;
            model.Filas = target.Filas;
            model.iIdCategoria = target.iIdCategoria;

            var response = await _httpService.Post<PostSaveNoticia, Response<Noticia>>(APIUrl, model);
            var validation = _validaServicioService.ValidadionServicio(response,
                onSuccess: data =>
                {
                    // Refresh list or update specific item
                    _ = PostGetNoticias(); 
                    if (data != null)
                    {
                        target.iIdNoticia = data.iIdNoticia;
                        target.vchTitulo = data.vchTitulo;
                        target.vchTituloEn = data.vchTituloEn;
                        target.nvchContenido = data.nvchContenido;
                        target.nvchContenidoEn = data.nvchContenidoEn;
                        target.vchImagenPortada = data.vchImagenPortada;
                        target.iIdCategoria = data.iIdCategoria;
                        target.bActivo = data.bActivo;
                        if (data.Filas != null && data.Filas.Any())
                        {
                            target.Filas = data.Filas;
                        }
                        if (data.Imagenes != null && data.Imagenes.Any())
                        {
                            target.Imagenes = data.Imagenes;
                        }
                    }
                    NoticiaSelected = data ?? target;
                });
            return validation;
        }

        public async Task<Response<BlobSaved>> PostUploadPortada(IBrowserFile file, string? titulo = null)
        {
            var urlAzureEndpoint = "api/AzureBlob/UploadImage-noticias";

            var rawTitle = !string.IsNullOrWhiteSpace(titulo) ? titulo : (!string.IsNullOrWhiteSpace(NoticiaSelected?.vchTitulo) ? NoticiaSelected.vchTitulo : "noticia");
            var safeTitle = rawTitle.Replace(" ", "_");
            var nombreUnico = $"{safeTitle}_{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
            using var memoryStream = new MemoryStream();
            await file.OpenReadStream(1024 * 1024 * 10).CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            using var imgContent = new StreamContent(memoryStream);

            var response = await _httpService.PostWithImage<Response<BlobSaved>>(urlAzureEndpoint, imgContent, nombreUnico);
            return response.Response ?? Response<BlobSaved>.BadResult("Error al subir la imagen al servidor.", new BlobSaved());
        }

        public async Task<SeverityMessage> UploadImage()
        {
            if (SelectedImage == null)
            {
                return new SeverityMessage(false, "Debe seleccionar una imagen.", SeverityTag.Error);
            }

            long maxFileSize = 1024L * 1024L * 5L; // 5MB

            using var memoryStream = new MemoryStream();
            await SelectedImage.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            StreamContent? img = new StreamContent(memoryStream);

            // Using existing AzureBlobController
            var response = await _httpService.PostWithFile<Response<BlobSaved>>(urlAzure, img);

            var validation = _validaServicioService.ValidadionServicio(response, ContinueWarning: false, ContinueError: false,
            onSuccess: data =>
            {
                var nuevaImagen = new NoticiaImagen { vchUrlImagen = data.URL };
                NoticiaSelected.Imagenes.Add(nuevaImagen);
                
                // If this is the first image, set it as cover
                if (string.IsNullOrEmpty(NoticiaSelected.vchImagenPortada))
                {
                    NoticiaSelected.vchImagenPortada = data.URL;
                }
            });

            return validation;
        }

        public async Task<string> UploadGenericFile(IBrowserFile file)
        {
            if (file == null) return string.Empty;

            long maxFileSize = 1024L * 1024L * 5L; // 5MB

            using var memoryStream = new MemoryStream();
            await file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            StreamContent? img = new StreamContent(memoryStream);

            var response = await _httpService.PostWithFile<Response<BlobSaved>>(urlAzure, img);
            string url = string.Empty;
            _validaServicioService.ValidadionServicio(response, ContinueWarning: false, ContinueError: false,
                onSuccess: data =>
                {
                    url = data.URL;
                });

            return url;
        }
    }
}
