using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.ConversationalModule;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class ConversationalDominio : IConversationalDominio
	{
		private readonly IConversationalRepository _conversationalRepository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public ConversationalDominio(
			IConversationalRepository conversationalRepository,
			MapperHelper mapperHelper,
			IResponseHelper responseHelper)
		{
			_conversationalRepository = conversationalRepository;
			_mapperHelper = mapperHelper ?? throw new ArgumentNullException(nameof(mapperHelper));
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
		}



		public async Task<Response<IEnumerable<Tramite>>> SearchTramitesForChat(PostSearchForChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostSearchForChat, spSearchTramitesForChat.Request>(postModel);
				var repoResult = await _conversationalRepository.spSearchTramitesForChat(req);
				var validation = _responseHelper.Validacion<spSearchTramitesForChat.Result, Tramite>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Tramite>>.BadResult(ex.Message, Enumerable.Empty<Tramite>());
			}
		}

		public async Task<Response<IEnumerable<Oficina>>> SearchOficinaForChat(PostSearchForChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostSearchForChat, spSearchOficinasForChat.Request>(postModel);
				var repoResult = await _conversationalRepository.spSearchOficinasForChat(req);
				var validation = _responseHelper.Validacion<spSearchOficinasForChat.Result, Oficina>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Oficina>>.BadResult(ex.Message, Enumerable.Empty<Oficina>());
			}
		}



		public async Task<Response<IEnumerable<Requisitos>>> SearchRequisitosForChat(PostGetByIdForChat postModel)
		{
			try
			{
				// Mapea el PostModel simple al Request del SP
				var req = _mapperHelper.Get<PostGetByIdForChat, spSearchRequisitosForChat.Request>(postModel);

				// Llama al Repositorio
				var repoResult = await _conversationalRepository.spSearchRequisitosForChat(req);

				// Valida y mapea la respuesta al modelo de Share (RequisitoDetalle)
				var validation = _responseHelper.Validacion<spSearchRequisitosForChat.Result, Requisitos>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Requisitos>>.BadResult(ex.Message, Enumerable.Empty<Requisitos>());
			}
		}


		public async Task<Response<IEnumerable<TramiteCosto>>> SearchCostoForChat(PostGetByIdForChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetByIdForChat, spSearchCostoForChat.Request>(postModel);

				var repoResult = await _conversationalRepository.spSearchCostoForChat(req);


				var validation = _responseHelper.Validacion<spSearchCostoForChat.Result, TramiteCosto>(repoResult);

				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<TramiteCosto>>.BadResult(ex.Message, Enumerable.Empty<TramiteCosto>());
			}
		}


		public async Task<Response<IEnumerable<Oficina>>> SearchOficinasByTramite(PostGetByIdForChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetByIdForChat, spSearchOficinasByTramite.Request>(postModel);
				var repoResult = await _conversationalRepository.spSearchOficinasByTramite(req);


				var validation = _responseHelper.Validacion<spSearchOficinasByTramite.Result, Oficina>(repoResult);

				return validation;
			}
			catch (Exception ex)
			{
				// 4. Manejo de Errores Estandarizado
				return Response<IEnumerable<Oficina>>.BadResult(ex.Message, Enumerable.Empty<Oficina>());
			}
		}

		public async Task<Response<IEnumerable<Chat>>> GetChat(PostGetChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetChat, spGetChat.Request>(postModel);
				var repoResult = await _conversationalRepository.spGetChat(req);
				var validation = _responseHelper.Validacion<spGetChat.Result, Chat>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Chat>>.BadResult(ex.Message, Enumerable.Empty<Chat>());
			}
		}

		public async Task<Response<Chat>> SaveChat(PostSaveChat postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostSaveChat, spSaveChat.Request>(postModel);
				var repoResult = await _conversationalRepository.spSaveChat(req);
				var validation = _responseHelper.Validacion<spSaveChat.Result, Chat>(repoResult);
				if (validation.IsSuccess)
				{
					postModel.iIdChat = validation.Data.iIdChat;
					validation.Data = _mapperHelper.Get<PostSaveChat, Chat>(postModel);
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<Chat>.BadResult(ex.Message, new Chat());
			}
		}

		public async Task<Response<IEnumerable<Mensaje>>> GetMensaje(PostGetMensaje postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetMensaje, spGetMensaje.Request>(postModel);
				var repoResult = await _conversationalRepository.spGetMensaje(req);
				var validation = _responseHelper.Validacion<spGetMensaje.Result, Mensaje>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Mensaje>>.BadResult(ex.Message, Enumerable.Empty<Mensaje>());
			}
		}

		public async Task<Response<Mensaje>> SaveMensaje(PostSaveMensaje postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostSaveMensaje, spSaveMensaje.Request>(postModel);
				var repoResult = await _conversationalRepository.spSaveMensaje(req);
				var validation = _responseHelper.Validacion<spSaveMensaje.Result, Mensaje>(repoResult);
				if (validation.IsSuccess)
				{
					postModel.iIdMensaje = validation.Data.iIdMensaje;
					validation.Data = _mapperHelper.Get<PostSaveMensaje, Mensaje>(postModel);
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<Mensaje>.BadResult(ex.Message, new Mensaje());
			}
		}
	}
}
