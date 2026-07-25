using Microsoft.SemanticKernel;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using EngramaCoreStandar.Extensions;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
	public class EmpresasPlugin
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public EmpresasPlugin(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		[KernelFunction]
		[Description("Busca empresas, comercios, negocios o servicios locales registrados en el municipio de Santiago Papasquiaro. Utiliza esta función cuando el usuario pregunte por restaurantes, hoteles, ferreterías, tiendas o cualquier negocio local.")]
		public async Task<string> BuscarEmpresas(
			[Description("Palabra clave para buscar empresas (ej. 'restaurante', 'hotel', 'ferretería', 'tienda'). Puede dejarse vacía para obtener todas las empresas activas.")]
			string query = "",
			[Description("Número máximo de resultados. Por defecto es 5.")]
			int limit = 5)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var result = await empresasDomain.GetEmpresas(new PostGetEmpresas { bEstatus = true });

			if (!result.IsSuccess || result.Data == null)
			{
				return "No se pudieron obtener las empresas en este momento.";
			}

			var empresas = result.Data;

			if (!string.IsNullOrWhiteSpace(query))
			{
				var cleanQuery = query.Trim().ToLower();
				empresas = empresas.Where(e =>
					(e.vchNombreComercial != null && e.vchNombreComercial.ToLower().Contains(cleanQuery)) ||
					(e.nvchDescripcion != null && e.nvchDescripcion.ToLower().Contains(cleanQuery))
				).ToList();
			}

			var resultado = empresas.Take(limit).Select(e => new
			{
				e.iIdEmpresa,
				e.vchNombreComercial,
				e.vchSlogan,
				e.vchTelefono,
				e.vchCorreo,
				e.vchLogoUrl
			}).ToList();

			if (resultado.Count == 0)
			{
				return $"No se encontraron empresas con la palabra clave '{query}'.";
			}

			return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
		}

		[KernelFunction]
		[Description("Obtiene la información detallada de una empresa específica del municipio incluyendo descripción, misión, visión e historia. Requiere el ID de la empresa obtenido de BuscarEmpresas.")]
		public async Task<string> GetEmpresaDetalle(
			[Description("El ID de la empresa a consultar.")]
			int idEmpresa)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var result = await empresasDomain.GetEmpresas(new PostGetEmpresas { iIdEmpresa = idEmpresa, bEstatus = true });

			if (!result.IsSuccess || result.Data == null || !result.Data.Any())
			{
				return $"No se encontró la empresa con ID {idEmpresa}.";
			}

			var empresa = result.Data.First();
			var resultado = new
			{
				empresa.iIdEmpresa,
				empresa.vchNombreComercial,
				empresa.vchSlogan,
				empresa.nvchDescripcion,
				empresa.nvchMision,
				empresa.nvchVision,
				empresa.nvchHistoria,
				empresa.vchTelefono,
				empresa.vchCorreo,
				empresa.vchLogoUrl
			};

			return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
		}

		[KernelFunction]
		[Description("Obtiene las ubicaciones físicas (direcciones, coordenadas) de una empresa del municipio. Utiliza esta función cuando el usuario pregunte por la dirección o la ubicación de un negocio.")]
		public async Task<string> GetEmpresaUbicaciones(
			[Description("El ID de la empresa para consultar sus ubicaciones.")]
			int idEmpresa)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var result = await empresasDomain.GetEmpresaUbicaciones(new PostGetEmpresaUbicaciones { iIdEmpresa = idEmpresa });

			if (!result.IsSuccess || result.Data == null || !result.Data.Any())
			{
				return $"No se encontraron ubicaciones para la empresa con ID {idEmpresa}.";
			}

			return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
		}

		[KernelFunction]
		[Description("Obtiene las redes sociales y enlaces web de una empresa del municipio (Facebook, Instagram, WhatsApp, sitio web, etc.).")]
		public async Task<string> GetEmpresaRedesSociales(
			[Description("El ID de la empresa para consultar sus redes sociales.")]
			int idEmpresa)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var result = await empresasDomain.GetEmpresaRedesSociales(new PostGetEmpresaRedesSociales { iIdEmpresa = idEmpresa });

			if (!result.IsSuccess || result.Data == null || !result.Data.Any())
			{
				return $"No se encontraron redes sociales para la empresa con ID {idEmpresa}.";
			}

			return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
		}

		[KernelFunction]
		[Description("Obtiene los productos y servicios que ofrece una empresa del municipio, organizados por categoría. Utiliza esta función cuando el usuario pregunte qué vende o qué servicios ofrece un negocio.")]
		public async Task<string> GetEmpresaProductosServicios(
			[Description("El ID de la empresa para consultar sus productos y servicios.")]
			int idEmpresa)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var categoriasRes = await empresasDomain.GetCategoriasPorEmpresa(new PostGetCategoriasPorEmpresa { iIdEmpresa = idEmpresa });

			if (!categoriasRes.IsSuccess || categoriasRes.Data == null || !categoriasRes.Data.Any())
			{
				return $"No se encontraron productos o servicios para la empresa con ID {idEmpresa}.";
			}

			var resultado = new List<object>();
			foreach (var cat in categoriasRes.Data)
			{
				var prodRes = await empresasDomain.GetProductosPorCategoria(new PostGetProductosPorCategoria { iIdCategoriaCat = cat.iIdCategoriaCat });
				if (prodRes.IsSuccess && prodRes.Data != null && prodRes.Data.Any())
				{
					resultado.Add(new
					{
						Categoria = cat.vchNombre,
						Productos = prodRes.Data.Select(p => new
						{
							p.iIdProducto,
							p.vchNombre,
							p.nvchDescripcionCorta,
							p.mPrecio,
							p.vchImagenUrl
						})
					});
				}
			}

			if (resultado.Count == 0)
			{
				return $"No se encontraron productos o servicios para la empresa con ID {idEmpresa}.";
			}

			return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
		}

		[KernelFunction]
		[Description("Busca servicios de emergencia registrados en el municipio: policía, bomberos, protección civil, rescate, seguridad, tránsito, control animal, ambulancias. Usa esta función cuando el usuario reporte una emergencia, incidente o necesite asistencia urgente (ej. incendio, accidente, animales peligrosos, delincuencia, inundación). Recomienda el servicio más adecuado según el tipo de incidente.")]
		public async Task<string> BuscarServiciosEmergencia(
			[Description("Descripción del incidente o tipo de asistencia que necesita el usuario (ej. 'incendio', 'accidente', 'choque', 'perro agresivo', 'robo', 'inundacion', 'derrame quimico').")]
			string incidente,
			[Description("Número máximo de resultados. Por defecto es 5.")]
			int limit = 5)
		{
			using var scope = _scopeFactory.CreateScope();
			var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

			var result = await empresasDomain.GetEmpresas(new PostGetEmpresas { bEstatus = true });

			if (!result.IsSuccess || result.Data == null)
			{
				return "No se pudieron obtener los servicios de emergencia en este momento.";
			}

			var serviciosEmergencia = result.Data.Where(e =>
				e.nvchDescripcion != null &&
				(e.nvchDescripcion.ToLower().Contains("emergencia") ||
				 e.nvchDescripcion.ToLower().Contains("seguridad") ||
				 e.nvchDescripcion.ToLower().Contains("policia") ||
				 e.nvchDescripcion.ToLower().Contains("bomber") ||
				 e.nvchDescripcion.ToLower().Contains("rescate") ||
				 e.nvchDescripcion.ToLower().Contains("proteccion civil") ||
				 e.nvchDescripcion.ToLower().Contains("transito") ||
				 e.nvchDescripcion.ToLower().Contains("ambulancia") ||
				 e.nvchDescripcion.ToLower().Contains("urgencia") ||
				 e.nvchDescripcion.ToLower().Contains("control animal") ||
				 e.vchNombreComercial.ToLower().Contains("policia") ||
				 e.vchNombreComercial.ToLower().Contains("bomber") ||
				 e.vchNombreComercial.ToLower().Contains("rescate") ||
				 e.vchNombreComercial.ToLower().Contains("seguridad") ||
				 e.vchNombreComercial.ToLower().Contains("transito") ||
				 e.vchNombreComercial.ToLower().Contains("proteccion civil") ||
				 e.vchNombreComercial.ToLower().Contains("ambulancia"))
			).ToList();

			if (!serviciosEmergencia.Any())
			{
				return "No se encontraron servicios de emergencia registrados en el sistema.";
			}

			var cleanIncidente = incidente.Trim().ToLower();
			var palabrasIncidente = cleanIncidente.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			var ranked = serviciosEmergencia
				.Select(e => new
				{
					Empresa = e,
					Relevancia = palabrasIncidente.Count(p =>
						(e.nvchDescripcion != null && e.nvchDescripcion.ToLower().Contains(p)) ||
						(e.vchNombreComercial != null && e.vchNombreComercial.ToLower().Contains(p)))
				})
				.OrderByDescending(x => x.Relevancia)
				.ThenBy(x => x.Empresa.vchNombreComercial)
				.Take(limit)
				.ToList();

			var options = new JsonSerializerOptions { WriteIndented = true };

			var resultado = ranked.Select(r =>
			{
				var ubiRes = empresasDomain.GetEmpresaUbicaciones(new PostGetEmpresaUbicaciones { iIdEmpresa = r.Empresa.iIdEmpresa }).Result;

				string recomendacion = ObtenerRecomendacion(r.Empresa.vchNombreComercial, r.Empresa.nvchDescripcion, cleanIncidente);

				return new
				{
					r.Empresa.iIdEmpresa,
					Servicio = r.Empresa.vchNombreComercial,
					r.Empresa.vchSlogan,
					r.Empresa.vchTelefono,
					r.Empresa.vchCorreo,
					Direccion = ubiRes.IsSuccess && ubiRes.Data != null && ubiRes.Data.Any()
						? ubiRes.Data.First().vchDireccion
						: null,
					Recomendacion = recomendacion,
					Relevancia = r.Relevancia
				};
			}).ToList();

			return JsonSerializer.Serialize(resultado, options);
		}

		private static string ObtenerRecomendacion(string nombre, string descripcion, string incidente)
		{
			var texto = (nombre + " " + descripcion).ToLower();

			if (incidente.Contains("fuego") || incidente.Contains("incendio") || incidente.Contains("quem") ||
				incidente.Contains("humo") || incidente.Contains("explos"))
				return texto.Contains("bomber") ? "Contacta a este cuerpo de bomberos para atender el incendio." : null;

			if (incidente.Contains("robo") || incidente.Contains("asalto") || incidente.Contains("delinc") ||
				incidente.Contains("sospech") || incidente.Contains("violenci") || incidente.Contains("pelea"))
				return texto.Contains("policia") || texto.Contains("seguridad") ? "Reporta el incidente a esta corporacion de seguridad." : null;

			if (incidente.Contains("accidente") || incidente.Contains("choque") || incidente.Contains("transito") ||
				incidente.Contains("vial") || incidente.Contains("atropell"))
				return texto.Contains("transito") || texto.Contains("policia") || texto.Contains("ambulancia")
					? "Notifica el accidente a esta autoridad para que acudan al lugar."
					: null;

			if (incidente.Contains("perro") || incidente.Contains("animal") || incidente.Contains("plaga") ||
				incidente.Contains("abej") || incidente.Contains("alacran") || incidente.Contains("serpient") ||
				incidente.Contains("roedor") || incidente.Contains("control animal"))
				return texto.Contains("control animal") || texto.Contains("bomber") || texto.Contains("rescate")
					? "Este servicio puede ayudarte con el control y manejo de animales."
					: null;

			if (incidente.Contains("inunda") || incidente.Contains("derrum") || incidente.Contains("deslave") ||
				incidente.Contains("huracan") || incidente.Contains("torment") || incidente.Contains("proteccion civil"))
				return texto.Contains("proteccion civil") || texto.Contains("bomber") || texto.Contains("rescate")
					? "Contacta a proteccion civil o rescate para atender esta contingencia."
					: null;

			if (incidente.Contains("herid") || incidente.Contains("lesion") || incidente.Contains("emergencia medica") ||
				incidente.Contains("ambulancia") || incidente.Contains("urgencia"))
				return texto.Contains("ambulancia") || texto.Contains("rescate") || texto.Contains("bomber")
					? "Solicita asistencia medica de emergencia a este servicio."
					: null;

			if (incidente.Contains("fuga") || incidente.Contains("derrame") || incidente.Contains("quimico") ||
				incidente.Contains("gas") || incidente.Contains("material peligro"))
				return texto.Contains("bomber") || texto.Contains("proteccion civil") || texto.Contains("rescate")
					? "Este servicio esta capacitado para atender materiales peligrosos y fugas."
					: null;

			return texto.Contains("policia") || texto.Contains("seguridad")
				? "Comunícate para recibir asistencia o reportar tu situación."
				: "Contacta a este servicio de emergencia para obtener ayuda.";
		}
	}
}
