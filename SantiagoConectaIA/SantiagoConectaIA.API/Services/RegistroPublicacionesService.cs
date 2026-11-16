using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.PublicacionesModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
	public static class PublicacionesTipos
	{
		public const string Noticia = "NOTICIA";
		public const string Emprendimiento = "EMPRENDIMIENTO";

		public const string Automatica = "AUTOMATICA";
		public const string Manual = "MANUAL";
		public const string Migracion = "MIGRACION";
	}

	/// <summary>Lo que ya pasó hoy (hora de México) con un tipo de publicación.</summary>
	/// <param name="Exitosas">Publicaciones exitosas de hoy, automáticas o manuales.</param>
	/// <param name="FallidasAutomaticas">Intentos automáticos fallidos de hoy.</param>
	/// <param name="UltimoIntentoAutomaticoUtc">Cuándo fue el último intento automático (exitoso o fallido).</param>
	public sealed record ResumenPublicacionesHoy(int Exitosas, int FallidasAutomaticas, DateTime? UltimoIntentoAutomaticoUtc);

	/// <summary>
	/// Registro en base de datos (SCIA.PublicacionesRegistro) de lo que se publica en redes sociales.
	/// Sustituye al archivo JSON de estado: no se pierde con un deploy y permite saber qué ya se publicó.
	/// </summary>
	public interface IRegistroPublicacionesService
	{
		/// <summary>Última publicación exitosa del tipo, o null si no hay ninguna.</summary>
		Task<PublicacionRegistro?> UltimaExitosaAsync(string tipo, CancellationToken cancellationToken = default);

		/// <summary>Resumen del día en curso. Lanza excepción si no se puede consultar la base.</summary>
		Task<ResumenPublicacionesHoy> ResumenHoyAsync(string tipo, CancellationToken cancellationToken = default);

		/// <summary>IDs que ya tienen una publicación exitosa.</summary>
		Task<HashSet<int>> IdsPublicadosAsync(string tipo, CancellationToken cancellationToken = default);

		/// <summary>Cuántos intentos automáticos fallidos tiene cada ID.</summary>
		Task<Dictionary<int, int>> FallosAutomaticosPorReferenciaAsync(string tipo, CancellationToken cancellationToken = default);

		/// <summary>Guarda el resultado de una publicación. No lanza excepción: si falla, lo deja en el log y devuelve false.</summary>
		Task<bool> RegistrarAsync(PostSavePublicacionRegistro registro, CancellationToken cancellationToken = default);
	}

	public class RegistroPublicacionesService : IRegistroPublicacionesService
	{
		private const int LimiteConsulta = 5000;

		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<RegistroPublicacionesService> _logger;

		public RegistroPublicacionesService(IServiceScopeFactory scopeFactory, ILogger<RegistroPublicacionesService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		public async Task<PublicacionRegistro?> UltimaExitosaAsync(string tipo, CancellationToken cancellationToken = default)
		{
			var filas = await ConsultarAsync(new PostGetPublicacionesRegistro { vchTipo = tipo, bExitoso = true, iTop = 1 });
			return filas.FirstOrDefault();
		}

		public async Task<ResumenPublicacionesHoy> ResumenHoyAsync(string tipo, CancellationToken cancellationToken = default)
		{
			var hoy = HorarioPublicacion.HoyLocal();
			var filas = await ConsultarAsync(new PostGetPublicacionesRegistro { vchTipo = tipo, dtDesde = hoy, dtHasta = hoy, iTop = 500 });

			var automaticas = filas.Where(f => f.vchOrigen == PublicacionesTipos.Automatica).ToList();
			return new ResumenPublicacionesHoy(
				filas.Count(f => f.bExitoso),
				automaticas.Count(f => !f.bExitoso),
				automaticas.Count == 0 ? null : automaticas.Max(f => f.dtPublicacion));
		}

		public async Task<HashSet<int>> IdsPublicadosAsync(string tipo, CancellationToken cancellationToken = default)
		{
			var filas = await ConsultarAsync(new PostGetPublicacionesRegistro { vchTipo = tipo, bExitoso = true, iTop = LimiteConsulta });
			return filas.Select(f => f.iIdReferencia).ToHashSet();
		}

		public async Task<Dictionary<int, int>> FallosAutomaticosPorReferenciaAsync(string tipo, CancellationToken cancellationToken = default)
		{
			var filas = await ConsultarAsync(new PostGetPublicacionesRegistro { vchTipo = tipo, bExitoso = false, iTop = LimiteConsulta });
			return filas
				.Where(f => f.vchOrigen == PublicacionesTipos.Automatica)
				.GroupBy(f => f.iIdReferencia)
				.ToDictionary(g => g.Key, g => g.Count());
		}

		public async Task<bool> RegistrarAsync(PostSavePublicacionRegistro registro, CancellationToken cancellationToken = default)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var dominio = scope.ServiceProvider.GetRequiredService<IPublicacionesRegistroDomain>();
				var resultado = await dominio.SavePublicacionRegistro(registro);
				if (!resultado.IsSuccess)
				{
					_logger.LogWarning("No se pudo guardar el registro de la publicación ({Tipo} #{Id}): {Mensaje}", registro.vchTipo, registro.iIdReferencia, resultado.Message);
				}

				return resultado.IsSuccess;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al guardar el registro de la publicación ({Tipo} #{Id}).", registro.vchTipo, registro.iIdReferencia);
				return false;
			}
		}

		private async Task<List<PublicacionRegistro>> ConsultarAsync(PostGetPublicacionesRegistro filtro)
		{
			using var scope = _scopeFactory.CreateScope();
			var dominio = scope.ServiceProvider.GetRequiredService<IPublicacionesRegistroDomain>();
			var resultado = await dominio.GetPublicacionesRegistro(filtro);
			if (!resultado.IsSuccess || resultado.Data == null)
			{
				throw new InvalidOperationException($"No se pudo consultar el registro de publicaciones: {resultado.Message}");
			}

			return resultado.Data.ToList();
		}
	}
}
