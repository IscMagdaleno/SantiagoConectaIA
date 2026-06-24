using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models;
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
		private readonly IConversationalDominio _conversationalDominio;
		private readonly EngramaContext _context;

		public ChatController(
			IAgentOrchestrationService agentOrchestrationService,
			IResponseHelper responseHelper,
			ILogger<OficinasController> logger,
			IConversationalDominio conversationalDominio,
			EngramaContext context)
		{
			_agentOrchestrationService = agentOrchestrationService;
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_conversationalDominio = conversationalDominio ?? throw new ArgumentNullException(nameof(conversationalDominio));
			_context = context ?? throw new ArgumentNullException(nameof(context));
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
				return BadRequest(Response<System.Collections.Generic.IEnumerable<SantiagoConectaIA.Share.Objects.OficinasModule.Oficina>>.BadResult("Error interno.", null));
			}
		}

		[HttpPost("PostGetChat")]
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
		public async Task<IActionResult> PostSaveChat([FromBody] PostSaveChat model)
		{
			var result = await _conversationalDominio.SaveChat(model);
			if (result.IsSuccess) return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("PostGetMensaje")]
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
		public async Task<IActionResult> PostSaveMensaje([FromBody] PostSaveMensaje model)
		{
			var result = await _conversationalDominio.SaveMensaje(model);
			if (result.IsSuccess) return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("PostSeedMockData")]
		public async Task<IActionResult> PostSeedMockData()
		{
			try
			{
				// Asegurar que el proyecto con ID 1 exista para evitar violaciones de clave foránea (FK_Chat_Proyecto)
				try
				{
					await _context.Database.ExecuteSqlRawAsync(@"
						IF NOT EXISTS (SELECT 1 FROM dbo.Proyecto WHERE iIdProyecto = 1)
						BEGIN
							SET IDENTITY_INSERT dbo.Proyecto ON;
							INSERT INTO dbo.Proyecto (iIdProyecto, nvchNombre, nvchDescripcion, dtCreadoEn, dtActualizadoEn)
							VALUES (1, 'Santiago Conecta', 'Asistente Virtual Inteligente de Santiago Papasquiaro', GETDATE(), GETDATE());
							SET IDENTITY_INSERT dbo.Proyecto OFF;
						END
					");
				}
				catch (Exception)
				{
					// Si falla por no ser columna identidad, intentar inserción directa
					try
					{
						await _context.Database.ExecuteSqlRawAsync(@"
							IF NOT EXISTS (SELECT 1 FROM dbo.Proyecto WHERE iIdProyecto = 1)
							BEGIN
								INSERT INTO dbo.Proyecto (iIdProyecto, nvchNombre, nvchDescripcion, dtCreadoEn, dtActualizadoEn)
								VALUES (1, 'Santiago Conecta', 'Asistente Virtual Inteligente de Santiago Papasquiaro', GETDATE(), GETDATE());
							END
						");
					}
					catch (Exception)
					{
						// Continuar de todos modos
					}
				}

				// Chat 1: Juan Pérez (Trámite de Acta de Nacimiento)
				var chat1 = new PostSaveChat
				{
					iIdProyecto = 1,
					nvchNombre = "Juan Pérez (Trámites)",
					bActivo = true,
					nvchThreadId = "thread_juan_perez_001",
					dtFechaCreacion = DateTime.Now.AddHours(-3)
				};
				var resChat1 = await _conversationalDominio.SaveChat(chat1);
				if (resChat1.IsSuccess)
				{
					int chatId = resChat1.Data.iIdChat;
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 1,
						nvchRol = "user",
						nvchContenido = "Hola, ¿cuánto cuesta el acta de nacimiento en el municipio y qué requisitos piden?",
						dtFecha = DateTime.Now.AddHours(-3).AddMinutes(2)
					});
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 2,
						nvchRol = "assistant",
						nvchContenido = "¡Hola! Con gusto te apoyo. El costo de la copia certificada de Acta de Nacimiento en Santiago Papasquiaro es de $180 MXN. Para obtenerla necesitas presentar:\n\n1. Nombre completo de la persona registrada.\n2. Fecha y lugar de nacimiento.\n3. Clave Única de Registro de Población (CURP) opcional para agilizar la búsqueda.\n\nPuedes acudir a las ventanillas del Registro Civil de Lunes a Viernes de 8:00 AM a 4:00 PM.",
						dtFecha = DateTime.Now.AddHours(-3).AddMinutes(3)
					});
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 3,
						nvchRol = "user",
						nvchContenido = "Perfecto, ¿y se puede pagar con tarjeta de crédito en ventanilla o solo efectivo?",
						dtFecha = DateTime.Now.AddHours(-3).AddMinutes(10)
					});
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 4,
						nvchRol = "assistant",
						nvchContenido = "Sí, en la ventanilla de Tesorería Municipal de Santiago Papasquiaro aceptamos tanto pagos en efectivo como con tarjeta de débito y crédito (Visa y MasterCard). ¡Te esperamos!",
						dtFecha = DateTime.Now.AddHours(-3).AddMinutes(11)
					});
				}

				// Chat 2: María Rodríguez (Pago de Impuesto Predial)
				var chat2 = new PostSaveChat
				{
					iIdProyecto = 1,
					nvchNombre = "María Rodríguez (Predial)",
					bActivo = true,
					nvchThreadId = "thread_maria_rodriguez_002",
					dtFechaCreacion = DateTime.Now.AddHours(-1)
				};
				var resChat2 = await _conversationalDominio.SaveChat(chat2);
				if (resChat2.IsSuccess)
				{
					int chatId = resChat2.Data.iIdChat;
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 1,
						nvchRol = "user",
						nvchContenido = "Buenas tardes, disculpe, ¿saben si hay algún descuento para el pago de predial?",
						dtFecha = DateTime.Now.AddHours(-1).AddMinutes(1)
					});
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 2,
						nvchRol = "assistant",
						nvchContenido = "¡Buenas tardes, María! Sí, contamos con atractivos descuentos para el Impuesto Predial de este año:\n\n• 15% de descuento por pronto pago durante todo el mes de Enero.\n• 10% de descuento durante el mes de Febrero.\n• 50% de descuento permanente para pensionados, jubilados, viudas y personas de la tercera edad (presentando credencial INAPAM/INSEN).\n\n¿Deseas que te ayude a calcular tu adeudo o conocer los puntos de recaudación?",
						dtFecha = DateTime.Now.AddHours(-1).AddMinutes(2)
					});
				}

				// Chat 3: WhatsApp Anónimo (Duda de Licencias)
				var chat3 = new PostSaveChat
				{
					iIdProyecto = 1,
					nvchNombre = "Anónimo (+52 618 456 7890)",
					bActivo = false,
					nvchThreadId = "thread_anon_003",
					dtFechaCreacion = DateTime.Now.AddMinutes(-30)
				};
				var resChat3 = await _conversationalDominio.SaveChat(chat3);
				if (resChat3.IsSuccess)
				{
					int chatId = resChat3.Data.iIdChat;
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 1,
						nvchRol = "user",
						nvchContenido = "Hola, ¿qué necesito para renovar mi licencia de conducir de chofer?",
						dtFecha = DateTime.Now.AddMinutes(-29)
					});
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = chatId,
						iOrden = 2,
						nvchRol = "assistant",
						nvchContenido = "¡Hola! Para renovar tu licencia de Conducir de Chofer en Santiago Papasquiaro, necesitas presentar los siguientes documentos en original y copia:\n\n1. Licencia anterior vencida.\n2. Identificación oficial vigente (INE o Pasaporte).\n3. Comprobante de domicilio reciente (no mayor a 3 meses).\n4. Examen médico de aptitud física y mental vigente.\n5. Comprobante de pago de derechos.\n\nEl trámite se realiza en la Dirección de Seguridad Pública y Vialidad Municipal.",
						dtFecha = DateTime.Now.AddMinutes(-28)
					});
				}

				return Ok(new Response<object> { IsSuccess = true, Data = "OK", Message = "Datos de prueba (3 chats con mensajes) sembrados exitosamente en la base de datos." });
			}
			catch (Exception ex)
			{
				return BadRequest(Response<object>.BadResult($"Error al sembrar datos de prueba: {ex.Message}", null));
			}
		}



	}
}
