using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using MudBlazor;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles
{
	public class DataTramites 
	{
		private string url = @"api/Tramites";
		#region INJECTS
		private readonly IHttpService _httpService;
		private readonly MapperHelper _mapper;
		private readonly IValidaServicioService _validaServicioService;
		#endregion
		#region PROPIEDADES
		public List<Tramite> LstTramites { get; set; }
		public Tramite TramiteSelected { get; set; }
		#endregion

		public DataTramites(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
		{
			_httpService = httpService;
			_mapper = mapper;
			_validaServicioService = validaServicioService;

			LstTramites = new List<Tramite>();
			TramiteSelected = new Tramite();
		}

		public async Task<SeverityMessage> PostGetTramites()
		{
			var APIUrl = url + "/PostGetTramites";

			var model = new PostGetTramites();
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

		public async Task<SeverityMessage> PostSaveTramites()
		{
			var APIUrl = url + "/PostSaveTramites";

			var model = new PostSaveTramite();
			var response = await _httpService.Post<PostSaveTramite, Response<Tramite>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => TramiteSelected = data);
			return validation;
		}

	}
}
