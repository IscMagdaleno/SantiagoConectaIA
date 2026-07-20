using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.WhatsAppModule;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class WhatsAppDomain : IWhatsAppDomain
	{
		private readonly IWhatsAppRepository _whatsAppRepository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public WhatsAppDomain(
			IWhatsAppRepository whatsAppRepository,
			MapperHelper mapperHelper,
			IResponseHelper responseHelper)
		{
			_whatsAppRepository = whatsAppRepository ?? throw new ArgumentNullException(nameof(whatsAppRepository));
			_mapperHelper = mapperHelper ?? throw new ArgumentNullException(nameof(mapperHelper));
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
		}

		public async Task<Response<WhatsAppUser>> SaveWhatsAppUser(PostSaveWhatsAppUser postModel)
		{
			try
			{
				var model = _mapperHelper.Get<PostSaveWhatsAppUser, spSaveWhatsAppUser.Request>(postModel);
				var result = await _whatsAppRepository.spSaveWhatsAppUser(model);
				var validation = _responseHelper.Validacion<spSaveWhatsAppUser.Result, WhatsAppUser>(result);

				if (validation.IsSuccess)
				{
					postModel.iIdWhatsAppUser = result.iIdWhatsAppUser ?? 0;
					validation.Data = _mapperHelper.Get<PostSaveWhatsAppUser, WhatsAppUser>(postModel);
					validation.Data.iIdWhatsAppUser = postModel.iIdWhatsAppUser;
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<WhatsAppUser>.BadResult(ex.Message, new WhatsAppUser());
			}
		}

		public async Task<Response<WhatsAppConversation>> SaveWhatsAppConversation(PostSaveWhatsAppConversation postModel)
		{
			try
			{
				var model = _mapperHelper.Get<PostSaveWhatsAppConversation, spSaveWhatsAppConversation.Request>(postModel);
				var result = await _whatsAppRepository.spSaveWhatsAppConversation(model);
				var validation = _responseHelper.Validacion<spSaveWhatsAppConversation.Result, WhatsAppConversation>(result);

				if (validation.IsSuccess)
				{
					postModel.iIdConversation = result.iIdConversation ?? 0;
					validation.Data = _mapperHelper.Get<PostSaveWhatsAppConversation, WhatsAppConversation>(postModel);
					validation.Data.iIdConversation = postModel.iIdConversation;
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<WhatsAppConversation>.BadResult(ex.Message, new WhatsAppConversation());
			}
		}

		public async Task<Response<WhatsAppMessage>> SaveWhatsAppMessage(PostSaveWhatsAppMessage postModel)
		{
			try
			{
				var model = _mapperHelper.Get<PostSaveWhatsAppMessage, spSaveWhatsAppMessage.Request>(postModel);
				var result = await _whatsAppRepository.spSaveWhatsAppMessage(model);
				var validation = _responseHelper.Validacion<spSaveWhatsAppMessage.Result, WhatsAppMessage>(result);

				if (validation.IsSuccess)
				{
					validation.Data = _mapperHelper.Get<PostSaveWhatsAppMessage, WhatsAppMessage>(postModel);
					validation.Data.iIdWhatsAppMessage = result.iIdWhatsAppMessage ?? 0;
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<WhatsAppMessage>.BadResult(ex.Message, new WhatsAppMessage());
			}
		}

		public async Task<Response<WhatsAppStats>> GetWhatsAppStats()
		{
			try
			{
				var request = new spGetWhatsAppStats.Request();
				var result = await _whatsAppRepository.spGetWhatsAppStats(request);
				var firstResult = result.FirstOrDefault();

				if (firstResult != null && firstResult.bResult)
				{
					return new Response<WhatsAppStats>
					{
						IsSuccess = true,
						Data = new WhatsAppStats
						{
							iTotalUsers = firstResult.iTotalUsers ?? 0,
							iTotalConversations = firstResult.iTotalConversations ?? 0,
							iTotalMessages = firstResult.iTotalMessages ?? 0,
							iActiveUsersToday = firstResult.iActiveUsersToday ?? 0,
							iActiveConversations = firstResult.iActiveConversations ?? 0,
							iMessagesToday = firstResult.iMessagesToday ?? 0,
							iNewUsersToday = firstResult.iNewUsersToday ?? 0,
							flAvgMessagesPerConversation = firstResult.flAvgMessagesPerConversation ?? 0
						},
						Message = "Ok"
					};
				}

				return new Response<WhatsAppStats>
				{
					IsSuccess = false,
					Data = new WhatsAppStats(),
					Message = firstResult?.vchMessage ?? "No data available"
				};
			}
			catch (Exception ex)
			{
				return Response<WhatsAppStats>.BadResult(ex.Message, new WhatsAppStats());
			}
		}

		public async Task<Response<IEnumerable<WhatsAppDailyStats>>> GetWhatsAppDailyStats(PostGetWhatsAppDailyStats postModel)
		{
			try
			{
				var model = _mapperHelper.Get<PostGetWhatsAppDailyStats, spGetWhatsAppDailyStats.Request>(postModel);
				var result = await _whatsAppRepository.spGetWhatsAppDailyStats(model);

				var dailyStats = result
					.Where(r => r.bResult)
					.Select(r => new WhatsAppDailyStats
					{
						dtDate = r.dtDate ?? DateTime.MinValue,
						iNewUsers = r.iNewUsers ?? 0,
						iMessagesInbound = r.iMessagesInbound ?? 0,
						iMessagesOutbound = r.iMessagesOutbound ?? 0,
						iTotalMessages = r.iTotalMessages ?? 0,
						iActiveConversations = r.iActiveConversations ?? 0
					});

				return new Response<IEnumerable<WhatsAppDailyStats>>
				{
					IsSuccess = true,
					Data = dailyStats,
					Message = "Ok"
				};
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<WhatsAppDailyStats>>.BadResult(ex.Message, Enumerable.Empty<WhatsAppDailyStats>());
			}
		}
	}
}
