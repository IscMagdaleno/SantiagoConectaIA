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

        public async Task<SeverityMessage> PostSaveNoticia()
        {
            var APIUrl = url + "/PostSaveNoticia";
            // Map NoticiaSelected to PostSaveNoticia
            var model = _mapper.Get<Noticia, PostSaveNoticia>(NoticiaSelected);
            
            // Ensure child lists and categories are included
            model.Imagenes = NoticiaSelected.Imagenes;
            model.Filas = NoticiaSelected.Filas;
            model.iIdCategoria = NoticiaSelected.iIdCategoria;

            var response = await _httpService.Post<PostSaveNoticia, Response<Noticia>>(APIUrl, model);
            var validation = _validaServicioService.ValidadionServicio(response,
                onSuccess: data =>
                {
                    // Refresh list or update specific item
                    PostGetNoticias(); 
                    NoticiaSelected = data;
                });
            return validation;
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
