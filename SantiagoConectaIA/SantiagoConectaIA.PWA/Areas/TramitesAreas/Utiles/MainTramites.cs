using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;

using Microsoft.AspNetCore.Components.Forms;

using SantiagoConectaIA.Share.Objects.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.OficinasModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles
{
	public class MainTramites
	{
		private string url = @"api/Tramites";
		private readonly string urlOficinas = "api/Oficinas";

		#region INJECTS
		private readonly IHttpService _httpService;
		private readonly MapperHelper _mapper;
		private readonly IValidaServicioService _validaServicioService;
		#endregion
		#region PROPIEDADES

		public List<Oficina> LstOficinas { get; set; }
		public List<Tramite> LstTramites { get; set; }


		public Tramite TramiteSelected { get; set; }

		public Documento DocumentoSelected { get; set; }

		public Oficina OficinaSelected { get; set; }

		public Requisitos RequisitoSelected { get; set; }

		public IBrowserFile SelectedFile { get; set; }


		#endregion

		public MainTramites(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
		{
			_httpService = httpService;
			_mapper = mapper;
			_validaServicioService = validaServicioService;

			LstTramites = new List<Tramite>();
			TramiteSelected = new Tramite();

			LstOficinas = new List<Oficina>();
			OficinaSelected = new Oficina();

			DocumentoSelected = new Documento();
			TramiteSelected.Documentos = new List<Documento>();

			RequisitoSelected = new Requisitos();
			TramiteSelected.Requisitos = new List<Requisitos>();
		}

		public async Task<SeverityMessage> PostGetTramites()
		{
			var APIUrl = url + "/PostGetTramites";

			var model = new PostGetTramites()
			{
				bActivo = true
			};
			var response = await _httpService.Post<PostGetTramites, Response<List<Tramite>>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstTramites = data.ToList());
			return validation;
		}

		public async Task<SeverityMessage> PostGetTramiteDetalle(int iIdTramite)
		{
			var APIUrl = url + "/PostGetTramiteDetalle";

			var model = new PostGetTramiteDetalle()
			{
				iIdTramite = iIdTramite
			};
			var response = await _httpService.Post<PostGetTramiteDetalle, Response<Tramite>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => TramiteSelected = data);
			return validation;
		}

		public async Task<SeverityMessage> PostSaveTramite(Tramite tramite)
		{
			var APIUrl = url + "/PostSaveTramite";
			var model = _mapper.Get<Tramite, PostSaveTramite>(tramite);
			var response = await _httpService.Post<PostSaveTramite, Response<Tramite>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => AfterSaveTramite(data));
			return validacion;

		}


		private void AfterSaveTramite(Tramite tramite)
		{
			TramiteSelected.iIdTramite = tramite.iIdTramite;

		}

		/// <summary>
		/// Consulta el API para obtener la lista de oficinas disponibles.
		/// </summary>
		public async Task<SeverityMessage> PostGetOficinas()
		{
			var APIUrl = urlOficinas + "/PostGetOficinas";
			var model = new PostGetOficinas();
			var response = await _httpService.Post<PostGetOficinas, Response<List<Oficina>>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstOficinas = data.ToList());
			return validation;
		}



		public async Task<SeverityMessage> PostSaveOficina()
		{
			var APIUrl = urlOficinas + "/PostSaveOficina";
			var model = _mapper.Get<Oficina, PostSaveOficina>(OficinaSelected);
			var response = await _httpService.Post<PostSaveOficina, Response<Oficina>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => LstOficinas.Add(data));
			return validacion;

		}

		public async Task<SeverityMessage> PostSaveDocumento()
		{
			DocumentoSelected.iIdTramite = TramiteSelected.iIdTramite;
			var APIUrl = url + "/PostSaveDocumento";
			var model = _mapper.Get<Documento, PostSaveDocumento>(DocumentoSelected);
			var response = await _httpService.Post<PostSaveDocumento, Response<Documento>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => TramiteSelected.Documentos.Add(data));
			return validacion;

		}

		public async Task<SeverityMessage> PostSaveRequisito()
		{
			RequisitoSelected.iIdTramite = TramiteSelected.iIdTramite;
			var APIUrl = url + "/PostSaveRequisito";
			var model = _mapper.Get<Requisitos, PostSaveRequisito>(RequisitoSelected);
			var response = await _httpService.Post<PostSaveRequisito, Response<Requisitos>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => TramiteSelected.Requisitos.Add(data));
			return validacion;

		}


		/// <summary>
		/// Sube el archivo seleccionado a la API de AzureBlobController y retorna la URL.
		/// </summary>
		public async Task<SeverityMessage> UploadFile()
		{
			if (SelectedFile == null)
			{
				return new SeverityMessage(false, "Debe seleccionar un archivo PDF para subir.", SeverityTag.Error);
			}

			// Validar tamaño y tipo de archivo antes de enviarlo
			if (SelectedFile.ContentType != "application/pdf")
			{
				return new SeverityMessage(false, "Solo se permiten archivos en formato PDF.", SeverityTag.Error);
			}

			var APIUrl = "api/AzureBlob/UploadDocument";

			long maxFileSize = 1024L * 1024L * 1024L * 2L;

			using var memoryStream = new MemoryStream();
			await SelectedFile.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
			memoryStream.Position = 0;


			StreamContent? pdf = new StreamContent(memoryStream);


			// Usar HttpService.PostFile (asumiendo que EngramaCoreStandard tiene este método)
			// PostFile maneja el multipart/form-data
			var response = await _httpService.PostWithFile<Response<BlobSaved>>(APIUrl, pdf);

			// Validar la respuesta del servicio. Si es exitoso, la URL estará en response.Response.Data
			var validation = _validaServicioService.ValidadionServicio(response, ContinueWarning: false, ContinueError: false,
			onSuccess: data =>
			{
				// Asignar la URL del blob al documento seleccionado en Main
				DocumentoSelected.vchUrlDocumento = data.URL;

			}
			);

			return validation;
		}


	}
}
