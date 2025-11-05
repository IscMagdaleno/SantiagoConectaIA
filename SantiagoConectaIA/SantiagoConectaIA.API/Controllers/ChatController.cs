using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

namespace SantiagoConectaIA.API.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class ChatController : ControllerBase
	{

		private readonly IAgentOrchestrationService _agentOrchestrationService;
		private readonly IResponseHelper _responseHelper;
		private readonly ILogger<OficinasController> _logger;

		public ChatController(
			IAgentOrchestrationService agentOrchestrationService,
			IResponseHelper responseHelper,
			ILogger<OficinasController> logger)
		{
			_agentOrchestrationService = agentOrchestrationService;
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}



		[HttpPost("PostSearchForChat")]
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
				_logger.LogError(ex, "Error en PostGetOficinas");
				return BadRequest(Response<System.Collections.Generic.IEnumerable<Oficina>>.BadResult("Error interno.", null));
			}
		}



	}
}
