using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

using System.Text.Json;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Hora e interruptor de las publicaciones automáticas. El interruptor sigue en un archivo JSON (App_Data);
	/// lo que ya se publicó vive en SCIA.PublicacionesRegistro (<see cref="IRegistroPublicacionesService"/>).
	/// </summary>
	public class PublicacionAutomaticaService : IPublicacionAutomaticaService
	{
		private const string ArchivoNoticias = "publicacion-automatica.json";
		private const string ArchivoEmprendimientos = "publicacion-automatica-emprendimientos.json";

		private static readonly TimeSpan HoraNoticiasPorDefecto = new(12, 30, 0);
		private static readonly TimeSpan HoraEmprendimientosPorDefecto = new(13, 0, 0);

		private readonly SemaphoreSlim _lock = new(1, 1);
		private readonly IHostEnvironment _environment;
		private readonly IParametrosService _parametros;
		private readonly IRegistroPublicacionesService _registro;
		private readonly ILogger<PublicacionAutomaticaService> _logger;
		private bool _archivoMigrado;

		public PublicacionAutomaticaService(
			IHostEnvironment environment,
			IParametrosService parametros,
			IRegistroPublicacionesService registro,
			ILogger<PublicacionAutomaticaService> logger)
		{
			_environment = environment;
			_parametros = parametros;
			_registro = registro;
			_logger = logger;
		}

		public async Task<TimeSpan> HoraPublicacionAsync(CancellationToken cancellationToken = default)
		{
			var valor = await _parametros.GetValor1Async(ParametrosAlias.PublicacionesAutoNoticias, cancellationToken);
			return HorarioPublicacion.LeerHora(valor, HoraNoticiasPorDefecto);
		}

		public async Task<TimeSpan> HoraEmprendimientosAsync(CancellationToken cancellationToken = default)
		{
			var valor = await _parametros.GetValor1Async(ParametrosAlias.PublicacionesAutoEmprendimientos, cancellationToken);
			return HorarioPublicacion.LeerHora(valor, HoraEmprendimientosPorDefecto);
		}

		public async Task<PublicacionAutomatica> GetAsync(CancellationToken cancellationToken = default)
		{
			await MigrarArchivoAsync(cancellationToken);
			var estado = await ConLockAsync(() => LeerAsync(ArchivoNoticias, () => new PublicacionAutomatica(), cancellationToken), cancellationToken);
			await CompletarNoticiasAsync(estado, cancellationToken);
			return estado;
		}

		public async Task<PublicacionAutomatica> SetActivoAsync(bool activo, CancellationToken cancellationToken = default)
		{
			await MigrarArchivoAsync(cancellationToken);
			var estado = await ActualizarAsync(ArchivoNoticias, () => new PublicacionAutomatica(), e => e.bActivo = activo, cancellationToken);
			await CompletarNoticiasAsync(estado, cancellationToken);
			return estado;
		}

		public async Task<PublicacionAutomaticaEmprendimientos> GetEmprendimientosAsync(CancellationToken cancellationToken = default)
		{
			await MigrarArchivoAsync(cancellationToken);
			var estado = await ConLockAsync(() => LeerAsync(ArchivoEmprendimientos, () => new PublicacionAutomaticaEmprendimientos(), cancellationToken), cancellationToken);
			await CompletarEmprendimientosAsync(estado, cancellationToken);
			return estado;
		}

		public async Task<PublicacionAutomaticaEmprendimientos> SetEmprendimientosActivoAsync(bool activo, CancellationToken cancellationToken = default)
		{
			await MigrarArchivoAsync(cancellationToken);
			var estado = await ActualizarAsync(ArchivoEmprendimientos, () => new PublicacionAutomaticaEmprendimientos(), e => e.bActivo = activo, cancellationToken);
			await CompletarEmprendimientosAsync(estado, cancellationToken);
			return estado;
		}

		/// <summary>Hora (parámetro) y última publicación (registro en base de datos).</summary>
		private async Task CompletarNoticiasAsync(PublicacionAutomatica estado, CancellationToken cancellationToken)
		{
			var hora = await HoraPublicacionAsync(cancellationToken);
			estado.iHora = hora.Hours;
			estado.iMinuto = hora.Minutes;
			estado.iIdUltimaNoticia = null;
			estado.vchUltimaNoticia = null;
			estado.dtUltimaPublicacion = null;

			try
			{
				var ultima = await _registro.UltimaExitosaAsync(PublicacionesTipos.Noticia, cancellationToken);
				if (ultima != null)
				{
					estado.iIdUltimaNoticia = ultima.iIdReferencia;
					estado.vchUltimaNoticia = ultima.vchTitulo;
					estado.dtUltimaPublicacion = ultima.dtPublicacion;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "No se pudo leer la última noticia publicada del registro.");
			}
		}

		/// <summary>Hora (parámetro), última publicación y siguiente de la rotación (registro en base de datos).</summary>
		private async Task CompletarEmprendimientosAsync(PublicacionAutomaticaEmprendimientos estado, CancellationToken cancellationToken)
		{
			var hora = await HoraEmprendimientosAsync(cancellationToken);
			estado.iHora = hora.Hours;
			estado.iMinuto = hora.Minutes;
			estado.iIdUltimaEmpresa = null;
			estado.vchUltimaEmpresa = null;
			estado.dtUltimaPublicacion = null;

			var inicial = int.TryParse(await _parametros.GetValor2Async(ParametrosAlias.PublicacionesAutoEmprendimientos, cancellationToken), out var id) && id > 0 ? id : 1;
			estado.iIdSiguienteEmpresa = inicial;

			try
			{
				var ultima = await _registro.UltimaExitosaAsync(PublicacionesTipos.Emprendimiento, cancellationToken);
				if (ultima != null)
				{
					estado.iIdUltimaEmpresa = ultima.iIdReferencia;
					estado.vchUltimaEmpresa = ultima.vchTitulo;
					estado.dtUltimaPublicacion = ultima.dtPublicacion;
					estado.iIdSiguienteEmpresa = ultima.iIdReferencia + 1;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "No se pudo leer el último emprendimiento publicado del registro.");
			}
		}

		/// <summary>
		/// Una sola vez: si los archivos JSON viejos recuerdan una publicación que el registro todavía no tiene,
		/// la copia al registro. Así, al estrenar la tabla no se repite la última noticia ni se reinicia la rotación.
		/// </summary>
		private async Task MigrarArchivoAsync(CancellationToken cancellationToken)
		{
			if (_archivoMigrado)
			{
				return;
			}

			await ConLockAsync(async () =>
			{
				if (_archivoMigrado)
				{
					return 0;
				}

				try
				{
					var noticias = await LeerAsync(ArchivoNoticias, () => new PublicacionAutomatica(), cancellationToken);
					if (noticias.iIdUltimaNoticia is > 0 && await _registro.UltimaExitosaAsync(PublicacionesTipos.Noticia, cancellationToken) == null)
					{
						await _registro.RegistrarAsync(RegistroMigrado(PublicacionesTipos.Noticia, noticias.iIdUltimaNoticia.Value, noticias.vchUltimaNoticia, noticias.dtUltimaPublicacion), cancellationToken);
					}

					var emprendimientos = await LeerAsync(ArchivoEmprendimientos, () => new PublicacionAutomaticaEmprendimientos(), cancellationToken);
					if (emprendimientos.iIdUltimaEmpresa is > 0 && await _registro.UltimaExitosaAsync(PublicacionesTipos.Emprendimiento, cancellationToken) == null)
					{
						await _registro.RegistrarAsync(RegistroMigrado(PublicacionesTipos.Emprendimiento, emprendimientos.iIdUltimaEmpresa.Value, emprendimientos.vchUltimaEmpresa, emprendimientos.dtUltimaPublicacion), cancellationToken);
					}

					_archivoMigrado = true;
				}
				catch (Exception ex)
				{
					// Si la base no responde se reintenta en la siguiente consulta.
					_logger.LogWarning(ex, "No se pudo migrar el estado de las publicaciones al registro.");
				}

				return 0;
			}, cancellationToken);
		}

		private static PostSavePublicacionRegistro RegistroMigrado(string tipo, int id, string? titulo, DateTime? fecha) => new()
		{
			vchTipo = tipo,
			iIdReferencia = id,
			vchTitulo = titulo,
			vchOrigen = PublicacionesTipos.Migracion,
			bExitoso = true,
			vchResultado = "Migrado del archivo de estado anterior.",
			dtPublicacion = fecha?.ToUniversalTime()
		};

		private async Task<T> ConLockAsync<T>(Func<Task<T>> accion, CancellationToken cancellationToken)
		{
			await _lock.WaitAsync(cancellationToken);
			try
			{
				return await accion();
			}
			finally
			{
				_lock.Release();
			}
		}

		private Task<T> ActualizarAsync<T>(string archivo, Func<T> crear, Action<T> cambio, CancellationToken cancellationToken)
		{
			return ConLockAsync(async () =>
			{
				var estado = await LeerAsync(archivo, crear, cancellationToken);
				cambio(estado);
				await GuardarAsync(archivo, estado, cancellationToken);
				return estado;
			}, cancellationToken);
		}

		private string Ruta(string archivo)
		{
			return Path.Combine(_environment.ContentRootPath, "App_Data", archivo);
		}

		private async Task<T> LeerAsync<T>(string archivo, Func<T> crear, CancellationToken cancellationToken)
		{
			var ruta = Ruta(archivo);
			if (!File.Exists(ruta))
			{
				return crear();
			}

			await using var stream = File.OpenRead(ruta);
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken) ?? crear();
		}

		private async Task GuardarAsync<T>(string archivo, T estado, CancellationToken cancellationToken)
		{
			var ruta = Ruta(archivo);
			Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
			await using var stream = File.Create(ruta);
			await JsonSerializer.SerializeAsync(stream, estado, cancellationToken: cancellationToken);
		}
	}
}
