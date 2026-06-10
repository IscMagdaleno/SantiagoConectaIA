using Microsoft.SemanticKernel;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using System.ComponentModel;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using EngramaCoreStandar.Extensions;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
	public class NoticiasPlugin
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public NoticiasPlugin(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		[KernelFunction]
		[Description("Busca noticias, novedades, avisos y eventos recientes en el municipio de Santiago Papasquiaro. Utiliza esta función si el usuario te pregunta qué hay de nuevo, qué eventos hay o sobre noticias municipales recientes.")]
		public async Task<string> BuscarNoticias(
			[Description("Palabra clave opcional para filtrar las noticias (ej. 'obras', 'festival', 'pago'). Puede dejarse vacía para obtener las noticias más recientes.")]
			string query = "")
		{
			using var scope = _scopeFactory.CreateScope();
			var noticiasDomain = scope.ServiceProvider.GetRequiredService<INoticiasDomain>();

			var respuesta = await noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = true });

			var options = new JsonSerializerOptions { WriteIndented = true };

			if (!respuesta.IsSuccess || respuesta.Data == null)
			{
				return "No se pudieron obtener las noticias del municipio en este momento.";
			}

			var noticias = respuesta.Data;

			// Si se proporcionó una consulta de búsqueda, filtramos localmente
			if (!string.IsNullOrWhiteSpace(query))
			{
				var cleanQuery = query.Trim().ToLower();
				noticias = noticias.Where(n => 
					(n.vchTitulo != null && n.vchTitulo.ToLower().Contains(cleanQuery)) ||
					(n.nvchContenido != null && n.nvchContenido.ToLower().Contains(cleanQuery))
				).ToList();
			}

			// Limitar a las últimas 5 noticias para no sobrecargar el contexto de tokens de Gemini
			var resultadoNoticias = noticias.Take(5).Select(n => new
			{
				n.iIdNoticia,
				n.vchTitulo,
				n.nvchContenido,
				FechaPublicacion = n.dtFechaPublicacion.ToString("dd/MM/yyyy")
			}).ToList();

			if (resultadoNoticias.Count == 0)
			{
				return $"No se encontraron noticias recientes con la palabra clave '{query}'.";
			}

			return JsonSerializer.Serialize(resultadoNoticias, options);
		}
	}
}
