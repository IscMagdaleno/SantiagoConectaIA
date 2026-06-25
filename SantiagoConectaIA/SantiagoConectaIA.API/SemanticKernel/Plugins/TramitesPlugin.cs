using EngramaCoreStandar.Extensions;

using Microsoft.SemanticKernel;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

using System.ComponentModel;

using System.Text.Json;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{


	public class ConsultaPlugin
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public ConsultaPlugin(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
		}


		/// <summary>
		/// Busca oficinas en el catálogo de SantiagoConectaIA basándose en una consulta de texto.
		/// </summary>
		/// <param name="query">La consulta de texto proporcionada por el usuario o el LLM para buscar Oficinas.</param>
		/// <param name="limit">El número máximo de oficinas a devolver. Por defecto es 5.</param>
		/// <returns>Una cadena JSON que representa una lista de oficinas encontrados. Si no se encuentran oficinas, devuelve una lista vacía.</returns>
		[KernelFunction]
		[Description("Busca oficinas en el catálogo de SantiagoConectaIA basándose en una consulta de texto. Utiliza esta función cuando el usuario pregunte por oficinas o necesite información sobre ellas.")]
		public async Task<string> SearchOficinas(
			[Description("La consulta de texto para buscar oficinas (ej. 'Catastro', 'Recaudación', 'Transito').")]
			string query,
			[Description("El número máximo de oficinas a devolver. Por defecto es 5.")]
			int limit = 5)
		{
			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();
			var OficinasEncontradas = await _conversationalDominio.SearchOficinaForChat(new PostSearchForChat { vchTexto = query, iLimit = limit });

			if (OficinasEncontradas.IsSuccess.False() || OficinasEncontradas.Data == null)
			{
				return $"No se encontraron oficinas para la consulta '{query}'.";
			}

			var resultado = OficinasEncontradas.Data.Select(o => new
			{
				o.iIdOficina,
				o.vchNombre,
				o.vchDireccion,
				o.vchTelefono,
				o.vchHorario
			});

			return JsonSerializer.Serialize(resultado);
		}


		/// <summary>
		/// Busca Tramites en el catálogo de SantiagoConectaIA basándose en una consulta de texto.
		/// </summary>
		/// <param name="query">La consulta de texto proporcionada por el usuario o el LLM para buscar tramites.</param>
		/// <param name="limit">El número máximo de productos a devolver. Por defecto es 5.</param>
		/// <returns>Una cadena JSON que representa una lista de oficinas encontrados. Si no se encuentran oficinas, devuelve una lista vacía.</returns>
		[KernelFunction]
		[Description("Busca tramites en el catálogo de SantiagoConectaIA basándose en una consulta de texto. Utiliza esta función cuando el usuario pregunte por tramites o necesite información sobre ellas.")]
		public async Task<string> SearchTramites(
			[Description("La consulta de texto para buscar tramites (ej. 'Licencia Conducir', 'Impuestos', 'Refrendo','Tenencia','Acta de Nacimiento', 'Pasaporte'). Si se envía vaso se podrán consultar todos los tramites")]
			string query,
			[Description("El número máximo de tramites a devolver. Por defecto es 5.")]
			int limit = 5)
		{
			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();
			var TramitesEncontrados = await _conversationalDominio.SearchTramitesForChat(new PostSearchForChat { vchTexto = query, iLimit = limit });

			if (TramitesEncontrados.IsSuccess.False() || TramitesEncontrados.Data == null)
			{
				return $"No se encontraron tramites para la consulta '{query}'.";
			}

			var resultado = TramitesEncontrados.Data.Select(t => new
			{
				t.iIdTramite,
				t.vchNombre,
				t.nvchDescripcion,
				t.bModalidadEnLinea,
				t.mCosto,
				t.bPrecioCalculado
			});

			return JsonSerializer.Serialize(resultado);
		}



		/// <summary>
		/// Busca los requisitos y documentos necesarios para un trámite específico.
		/// </summary>
		/// <param name="idTramite">El ID del trámite a buscar (obtenido de una consulta previa a SearchTramites).</param>
		/// <returns>Una cadena JSON que representa la lista de requisitos y documentos.</returns>
		[KernelFunction]
		[Description("Busca los requisitos y documentos necesarios para un trámite específico. Utiliza esta función cuando el usuario pida la documentación o requisitos para un trámite en particular. Requiere el ID del trámite.")]
		public async Task<string> SearchRequisitos(
			[Description("El ID del trámite para el cual se buscan los requisitos.")]
	int idTramite)
		{
			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();
			var RequisitosEncontrados = await _conversationalDominio.SearchRequisitosForChat(new PostGetByIdForChat { iIdTramite = idTramite });

			if (RequisitosEncontrados.IsSuccess.False() || RequisitosEncontrados.Data == null)
			{
				return $"No se encontraron requisitos para el ID de trámite {idTramite}.";
			}

			var resultado = RequisitosEncontrados.Data.Select(r => new
			{
				r.iIdRequisito,
				r.vchNombre,
				r.nvchDetalle,
				r.bObligatorio
			});

			return JsonSerializer.Serialize(resultado);
		}




		/// <summary>
		/// Busca el costo y la modalidad en línea (si aplica) de un trámite específico.
		/// </summary>
		/// <param name="idTramite">El ID del trámite a buscar (obtenido de una consulta previa a SearchTramites).</param>
		/// <returns>Una cadena JSON que representa el costo y la modalidad del trámite. Si no se encuentra, devuelve un mensaje de error.</returns>
		[KernelFunction]
		[Description("Busca el costo monetario y si el trámite se puede realizar en línea. Utiliza esta función cuando el usuario pregunte por el precio o la modalidad (presencial/en línea) de un trámite en particular. Requiere el ID del trámite.")]
		public async Task<string> SearchCosto(
			[Description("El ID del trámite para el cual se busca el costo.")]
	int idTramite)
		{
			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();
			var CostoEncontrado = await _conversationalDominio.SearchCostoForChat(new PostGetByIdForChat { iIdTramite = idTramite });

			if (CostoEncontrado.IsSuccess.False() || CostoEncontrado.Data == null)
			{
				return $"No se encontró información de costo para el ID de trámite {idTramite}.";
			}

			var resultado = CostoEncontrado.Data.Select(c => new
			{
				c.mCosto,
				c.bPrecioCalculado,
				c.bModalidadEnLinea
			});

			return JsonSerializer.Serialize(resultado);
		}


		// ... en ConsultaPlugin

		/// <summary>
		/// Busca la lista completa de oficinas donde se puede realizar un trámite específico.
		/// Esta función resuelve la relación Muchos a Muchos (N:M).
		/// </summary>
		/// <param name="idTramite">El ID del trámite (obtenido de una consulta previa a SearchTramites).</param>
		/// <returns>Una cadena JSON que representa una lista de oficinas (dirección, horario, etc.).</returns>
		[KernelFunction]
		[Description("Busca todas las oficinas (direcciones, horarios) asociadas a un trámite específico, resolviendo la relación de múltiples oficinas por trámite. Utiliza esta función cuando el usuario pregunte por la ubicación o dónde realizar un trámite en particular. Requiere el ID del trámite.")]
		public async Task<string> SearchOficinasByTramite(
			[Description("El ID del trámite para el cual se buscan las oficinas.")]
	int idTramite)
		{
			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();
			var OficinasEncontradas = await _conversationalDominio.SearchOficinasByTramite(new PostGetByIdForChat { iIdTramite = idTramite });

			if (OficinasEncontradas.IsSuccess.False() || OficinasEncontradas.Data == null)
			{
				return $"No se encontraron oficinas para el ID de trámite {idTramite}.";
			}

			var resultado = OficinasEncontradas.Data.Select(o => new
			{
				o.iIdOficina,
				o.vchNombre,
				o.vchDireccion,
				o.vchTelefono,
				o.vchHorario
			});

			return JsonSerializer.Serialize(resultado);
		}
	}
}
