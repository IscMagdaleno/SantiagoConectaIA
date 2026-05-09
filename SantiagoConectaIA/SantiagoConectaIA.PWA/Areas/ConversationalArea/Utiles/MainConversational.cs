using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objects.ConversationalModule;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

namespace SantiagoConectaIA.PWA.Areas.ConversationalArea.Utiles
{
	public class MainConversational
	{
		private readonly string url = "api/Chat";
		private readonly IHttpService _httpService;
		private readonly MapperHelper _mapper;
		private readonly IValidaServicioService _validaServicioService;

		public List<Chat> LstChats { get; set; } = new List<Chat>();
		public List<Mensaje> LstMensajes { get; set; } = new List<Mensaje>();

		public Chat ChatSelected { get; set; } = new Chat();
		public Mensaje MensajeSelected { get; set; } = new Mensaje();

		public MainConversational(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
		{
			_httpService = httpService;
			_mapper = mapper;
			_validaServicioService = validaServicioService;
		}

		public async Task<SeverityMessage> PostGetChats(int iIdProyecto = 1)
		{
			var apiUrl = $"{url}/PostGetChat";
			var model = new PostGetChat { iIdProyecto = iIdProyecto };

			var response = await _httpService.Post<PostGetChat, Response<List<Chat>>>(apiUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, 
				onSuccess: data => LstChats = data.ToList());
			return validation;
		}

		public async Task<SeverityMessage> PostGetMensajes(int iIdChat)
		{
			var apiUrl = $"{url}/PostGetMensaje";
			var model = new PostGetMensaje { iIdChat = iIdChat };

			var response = await _httpService.Post<PostGetMensaje, Response<List<Mensaje>>>(apiUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, 
				onSuccess: data => LstMensajes = data.ToList());
			return validation;
		}

		public async Task<SeverityMessage> PostSaveChat(Chat chat)
		{
			var apiUrl = $"{url}/PostSaveChat";
			var model = _mapper.Get<Chat, PostSaveChat>(chat);

			var response = await _httpService.Post<PostSaveChat, Response<Chat>>(apiUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, 
				onSuccess: data => ChatSelected = data);
			return validation;
		}

		public async Task<SeverityMessage> PostSaveMensaje(Mensaje mensaje)
		{
			var apiUrl = $"{url}/PostSaveMensaje";
			var model = _mapper.Get<Mensaje, PostSaveMensaje>(mensaje);

			var response = await _httpService.Post<PostSaveMensaje, Response<Mensaje>>(apiUrl, model);
			var validation = _validaServicioService.ValidadionServicio(response, 
				onSuccess: data => MensajeSelected = data);
			return validation;
		}
	}
}
