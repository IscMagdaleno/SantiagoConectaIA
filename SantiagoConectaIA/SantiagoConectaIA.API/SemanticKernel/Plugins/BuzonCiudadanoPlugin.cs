using Microsoft.SemanticKernel;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
	public class BuzonCiudadanoPlugin
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public BuzonCiudadanoPlugin(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		[KernelFunction]
		[Description("Registra un reporte, queja o sugerencia de un ciudadano en el Buzón Ciudadano municipal (ej. reporte de baches, luminarias fundidas de alumbrado público, fugas de agua potable, basura, etc.). Solicita al ciudadano los datos requeridos (nombre, categoría, descripción y opcionalmente teléfono/email) antes de llamar a esta función.")]
		public async Task<string> RegistrarReporte(
			[Description("El nombre completo del ciudadano.")]
			string nombreCiudadano,
			[Description("La categoría del reporte (ej. 'Alumbrado', 'Baches', 'Fuga de agua', 'Basura', 'Seguridad', 'Otro').")]
			string categoria,
			[Description("Descripción detallada del problema, reportes o sugerencia.")]
			string descripcion,
			[Description("Correo electrónico opcional de contacto.")]
			string email = "",
			[Description("Número de teléfono o celular opcional de contacto.")]
			string telefono = "")
		{
			using var scope = _scopeFactory.CreateScope();
			var buzonDomain = scope.ServiceProvider.GetRequiredService<IBuzonCiudadanoDomain>();

			var postModel = new PostSaveBuzonCiudadano
			{
				nvchNombreCiudadano = nombreCiudadano,
				nvchCategoria = categoria,
				nvchDescripcion = descripcion,
				nvchEmail = string.IsNullOrWhiteSpace(email) ? null : email,
				nvchTelefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono
			};

			var respuesta = await buzonDomain.RegistrarReporte(postModel);

			var options = new JsonSerializerOptions { WriteIndented = true };

			if (!respuesta.IsSuccess || respuesta.Data == null)
			{
				return $"No se pudo registrar el reporte en el buzón ciudadano en este momento. Detalles: {respuesta.Message}";
			}

			return JsonSerializer.Serialize(new
			{
				bResult = true,
				vchMessage = "El reporte ha sido registrado de manera exitosa en el buzón municipal.",
				iIdReporte = respuesta.Data.iIdReporte,
				FechaRegistro = respuesta.Data.dtFechaReporte.ToString("dd/MM/yyyy HH:mm")
			}, options);
		}
	}
}
