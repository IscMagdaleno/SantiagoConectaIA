using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

namespace SantiagoConectaIA.API.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class ChatController : ApiControllerBase
	{

		private readonly IAgentOrchestrationService _agentOrchestrationService;
		private readonly ILogger<ChatController> _logger;
		private readonly IConversationalDominio _conversationalDominio;

		public ChatController(
			IAgentOrchestrationService agentOrchestrationService,
			ILogger<ChatController> logger,
			IConversationalDominio conversationalDominio)
		{
			_agentOrchestrationService = agentOrchestrationService;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_conversationalDominio = conversationalDominio ?? throw new ArgumentNullException(nameof(conversationalDominio));
		}

		[HttpPost("PostSearchForChat")]
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Chat)]
		public async Task<IActionResult> PostSearchForChat([FromBody] PostChatRequest model)
		{
			try
			{
				var result = await _agentOrchestrationService.ProcessUserQueryAsync(model.nvchUserQuery, model.nvchUserId);
				if (result.NotNull())
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSearchForChat");
				return BadRequest(Response<object>.BadResult("Error interno.", null));
			}
		}

		[HttpPost("PostGetChat")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostGetChat([FromBody] PostGetChat model)
		{
			var result = await _conversationalDominio.GetChat(model);
			if (result.IsSuccess || result.Message == "No register found.")
			{
				if (!result.IsSuccess)
				{
					result.IsSuccess = true;
					result.Data = Enumerable.Empty<SantiagoConectaIA.Share.Objects.ConversationalModule.Chat>();
				}
				return Ok(result);
			}
			return BadRequest(result);
		}

		[HttpPost("PostSaveChat")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveChat([FromBody] PostSaveChat model)
		{
			var result = await _conversationalDominio.SaveChat(model);
			return Resultado(result);
		}

		[HttpPost("PostGetMensaje")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostGetMensaje([FromBody] PostGetMensaje model)
		{
			var result = await _conversationalDominio.GetMensaje(model);
			if (result.IsSuccess || result.Message == "No register found.")
			{
				if (!result.IsSuccess)
				{
					result.IsSuccess = true;
					result.Data = Enumerable.Empty<SantiagoConectaIA.Share.Objects.ConversationalModule.Mensaje>();
				}
				return Ok(result);
			}
			return BadRequest(result);
		}

		[HttpPost("PostSaveMensaje")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveMensaje([FromBody] PostSaveMensaje model)
		{
			var result = await _conversationalDominio.SaveMensaje(model);
			return Resultado(result);
		}
	}
}
