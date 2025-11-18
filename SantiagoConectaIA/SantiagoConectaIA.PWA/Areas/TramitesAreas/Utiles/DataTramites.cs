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
	public class DataTramites
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

		public List<Oficina> LstOficinas { get; set; }

		public Oficina OficinaSelected { get; set; }


		#endregion

		public DataTramites(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
		{
			_httpService = httpService;
			_mapper = mapper;
			_validaServicioService = validaServicioService;

			LstTramites = new List<Tramite>();
			TramiteSelected = new Tramite();

			LstOficinas = new List<Oficina>();
			OficinaSelected = new Oficina();
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
			var response = await _httpService.Post<PostGetTramites, Response<List<Tramite>>>(APIUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstTramites = data.ToList());
			return validation;
		}

		public async Task<SeverityMessage> PostSaveTramite()
		{
			var APIUrl = url + "/PostSaveTramite";
			var model = _mapper.Get<Tramite, PostSaveTramite>(TramiteSelected);
			var response = await _httpService.Post<PostSaveTramite, Response<Tramite>>(APIUrl, model);
			var validacion = _validaServicioService.ValidadionServicio(response,
			onSuccess: data => LstTramites.Add(data));
			return validacion;

		}

		/// <summary>
		/// Consulta el API para obtener la lista de oficinas disponibles.
		/// </summary>
		public async Task<SeverityMessage> PostGetOficinas()
		{
			// 1. Definir la URL completa del endpoint
			var APIUrl = urlOficinas + "/PostGetOficinas";

			// 2. Crear el PostModel de solicitud. 
			// Se asume que PostGetOficinas no requiere parámetros complejos por defecto.
			var model = new PostGetOficinas();

			// 3. Invocar el servicio HTTP (IHttpService)
			var response = await _httpService.Post<PostGetOficinas, Response<List<Oficina>>>(APIUrl, model);

			// 4. Validar la respuesta y actualizar la propiedad LstOficinas
			// Usa ValidadionServicio (con 'd' extra, según tu ejemplo)
			var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstOficinas = data.ToList());

			return validation;
		}

	}
}
