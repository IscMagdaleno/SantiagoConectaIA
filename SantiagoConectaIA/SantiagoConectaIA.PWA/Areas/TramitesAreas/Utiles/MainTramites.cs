using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;

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
		public List<Tramite> LstTramites { get; set; }
		public Tramite TramiteSelected { get; set; }

		public List<Documento> LstDocumentos { get; set; }
		public Documento DocumentoSelected { get; set; }

		public List<Oficina> LstOficinas { get; set; }
		public Oficina OficinaSelected { get; set; }



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
			LstDocumentos = new List<Documento>();
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

		public async Task<SeverityMessage> PostGetTramiteDetail()
		{
			var APIUrl = url + "/PostGetTramiteDetail";

			var model = new PostGetTramites();
			var response = await _httpService.Post<PostGetTramites, Response<Tramite>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => TramiteSelected = data);
			return validation;
		}

		public async Task<SeverityMessage> PostSaveTramite()
		{
			var APIUrl = url + "/PostSaveTramite";
			var model = _mapper.Get<Tramite, PostSaveTramite>(TramiteSelected);
			var response = await _httpService.Post<PostSaveTramite, Response<Tramite>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => TramiteSelected = (data));
			return validacion;

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
			onSuccess: data => LstDocumentos.Add(data));
			return validacion;

		}


	}
}
