using Microsoft.Extensions.Caching.Memory;

using SantiagoConectaIA.Share.Objects.MemesModule;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SantiagoConectaIA.API.Services
{
	public sealed class MemePlantillasClient : IMemePlantillasClient
	{
		private const string BaseUrlPorDefecto = "https://api.createa.meme/v1";
		private const int MaxPaginasBusqueda = 6;
		private const int TamanoPagina = 100;

		private static readonly TimeSpan VigenciaListas = TimeSpan.FromMinutes(5);
		private static readonly TimeSpan VigenciaBusquedas = TimeSpan.FromSeconds(60);
		private static readonly TimeSpan VigenciaPlantilla = TimeSpan.FromHours(6);

		private static readonly HashSet<string> Periodos = new(StringComparer.OrdinalIgnoreCase) { "24h", "7d", "30d" };

		private readonly HttpClient _http;
		private readonly IParametrosService _parametros;
		private readonly IMemoryCache _cache;
		private readonly ILogger<MemePlantillasClient> _logger;

		public MemePlantillasClient(HttpClient http, IParametrosService parametros, IMemoryCache cache, ILogger<MemePlantillasClient> logger)
		{
			_http = http;
			_parametros = parametros;
			_cache = cache;
			_logger = logger;
		}

		public async Task<PlantillasResultado> ListarAsync(string modo, string? busqueda, string? categoria, string periodo, int limite, int desde, CancellationToken cancellationToken = default)
		{
			var esTendencia = string.Equals(modo, ModosMeme.Tendencias, StringComparison.OrdinalIgnoreCase);
			limite = Math.Clamp(limite, 1, 100);
			desde = Math.Max(0, desde);
			periodo = Periodos.Contains(periodo ?? string.Empty) ? periodo!.ToLowerInvariant() : "7d";
			var categoriaLimpia = LimpiarFiltro(categoria, 30);
			var busquedaLimpia = LimpiarFiltro(busqueda, 100);

			var llave = $"meme:lista:{(esTendencia ? "t" : "b")}:{periodo}:{categoriaLimpia}:{busquedaLimpia}:{limite}:{desde}";
			if (_cache.TryGetValue(llave, out PlantillasResultado? guardada) && guardada != null)
			{
				return guardada;
			}

			var consulta = esTendencia
				? $"trending?timeframe={periodo}&limit={limite}" + (categoriaLimpia.Length > 0 ? $"&category={Uri.EscapeDataString(categoriaLimpia)}" : string.Empty)
				: $"templates?limit={limite}&offset={desde}"
				  + (busquedaLimpia.Length > 0 ? $"&q={Uri.EscapeDataString(busquedaLimpia)}" : string.Empty)
				  + (categoriaLimpia.Length > 0 ? $"&category={Uri.EscapeDataString(categoriaLimpia)}" : string.Empty);

			var resultado = await ConsultarAsync(consulta, cancellationToken);
			if (resultado.Exitosa)
			{
				foreach (var plantilla in resultado.Lista.Plantillas)
				{
					RecordarPlantilla(plantilla);
				}

				_cache.Set(llave, resultado, busquedaLimpia.Length > 0 ? VigenciaBusquedas : VigenciaListas);
			}

			return resultado;
		}

		public async Task<PlantillaMeme?> ObtenerAsync(string id, CancellationToken cancellationToken = default)
		{
			id = (id ?? string.Empty).Trim();
			if (id.Length == 0 || id.Length > 80)
			{
				return null;
			}

			if (_cache.TryGetValue(LlavePlantilla(id), out PlantillaMeme? conocida) && conocida != null)
			{
				return conocida;
			}

			// No estaba en memoria (se reinició la API o venció): se recorre la biblioteca hasta encontrarla.
			for (var pagina = 0; pagina < MaxPaginasBusqueda; pagina++)
			{
				var respuesta = await ConsultarAsync($"templates?limit={TamanoPagina}&offset={pagina * TamanoPagina}", cancellationToken);
				if (!respuesta.Exitosa)
				{
					return null;
				}

				foreach (var plantilla in respuesta.Lista.Plantillas)
				{
					RecordarPlantilla(plantilla);
				}

				if (_cache.TryGetValue(LlavePlantilla(id), out conocida) && conocida != null)
				{
					return conocida;
				}

				if (respuesta.Lista.Plantillas.Count < TamanoPagina)
				{
					break;
				}
			}

			return null;
		}

		private static string LlavePlantilla(string id) => "meme:tpl:" + id.ToLowerInvariant();

		private void RecordarPlantilla(PlantillaMeme plantilla) => _cache.Set(LlavePlantilla(plantilla.vchId), plantilla, VigenciaPlantilla);

		private static string LimpiarFiltro(string? texto, int max)
		{
			if (string.IsNullOrWhiteSpace(texto))
			{
				return string.Empty;
			}

			var limpio = new string(texto.Where(c => !char.IsControl(c)).ToArray()).Trim();
			return limpio.Length > max ? limpio[..max] : limpio;
		}

		private async Task<PlantillasResultado> ConsultarAsync(string rutaYConsulta, CancellationToken cancellationToken)
		{
			var llave = await _parametros.GetValor2Async(ParametrosAlias.MemesPlantillas, cancellationToken);
			if (string.IsNullOrWhiteSpace(llave))
			{
				return PlantillasResultado.Falla($"Los memes aún no están configurados: falta la llave en el parámetro {ParametrosAlias.MemesPlantillas} (Valor2).");
			}

			var baseUrl = ((await _parametros.GetValor1Async(ParametrosAlias.MemesPlantillas, cancellationToken)) ?? BaseUrlPorDefecto).TrimEnd('/');
			if (!Uri.TryCreate(baseUrl + "/" + rutaYConsulta, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
			{
				return PlantillasResultado.Falla("La URL del servicio de memes no es válida.");
			}

			try
			{
				using var solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
				solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", llave);
				solicitud.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				using var respuesta = await _http.SendAsync(solicitud, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
				if (!respuesta.IsSuccessStatusCode)
				{
					// Nunca se registra la llave ni el cuerpo de la respuesta.
					_logger.LogWarning("El servicio de memes respondió {Estado} en {Ruta}.", (int)respuesta.StatusCode, uri.AbsolutePath);
					return PlantillasResultado.Falla(respuesta.StatusCode switch
					{
						HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => $"La llave del servicio de memes no es válida. Revisa el parámetro {ParametrosAlias.MemesPlantillas}.",
						HttpStatusCode.TooManyRequests => "El servicio de memes recibió demasiadas solicitudes. Inténtalo en un momento.",
						_ => "No se pudieron cargar las plantillas de memes."
					});
				}

				await using var flujo = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
				using var documento = await JsonDocument.ParseAsync(flujo, cancellationToken: cancellationToken);
				return new PlantillasResultado { Exitosa = true, Lista = Leer(documento.RootElement) };
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				return PlantillasResultado.Falla("El servicio de memes tardó demasiado en responder.");
			}
			catch (Exception ex) when (ex is HttpRequestException or JsonException)
			{
				_logger.LogWarning(ex, "No se pudo consultar el servicio de memes.");
				return PlantillasResultado.Falla("No se pudieron cargar las plantillas de memes.");
			}
		}

		/// <summary>
		/// Lectura tolerante: el documento publica <c>data.templates[]</c> para la biblioteca y una lista similar para tendencias.
		/// Se toma la primera lista de objetos con <c>id</c> e <c>image_url</c> (directo o dentro de <c>template</c>).
		/// </summary>
		private static ListaPlantillasMeme Leer(JsonElement raiz)
		{
			var lista = new ListaPlantillasMeme();
			var data = raiz.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object ? d : raiz;
			if (data.ValueKind != JsonValueKind.Object)
			{
				return lista;
			}

			JsonElement? arreglo = null;
			foreach (var nombre in new[] { "templates", "trending", "items" })
			{
				if (data.TryGetProperty(nombre, out var a) && a.ValueKind == JsonValueKind.Array)
				{
					arreglo = a;
					break;
				}
			}

			if (arreglo == null)
			{
				foreach (var propiedad in data.EnumerateObject())
				{
					if (propiedad.Value.ValueKind == JsonValueKind.Array && propiedad.Value.EnumerateArray().Any(e => e.ValueKind == JsonValueKind.Object))
					{
						arreglo = propiedad.Value;
						break;
					}
				}
			}

			if (arreglo != null)
			{
				foreach (var elemento in arreglo.Value.EnumerateArray())
				{
					var origen = elemento.ValueKind == JsonValueKind.Object && elemento.TryGetProperty("template", out var t) && t.ValueKind == JsonValueKind.Object
						? t
						: elemento;
					var plantilla = LeerPlantilla(origen);
					if (plantilla != null)
					{
						lista.Plantillas.Add(plantilla);
					}
				}
			}

			lista.iTotal = data.TryGetProperty("total", out var total) && total.ValueKind == JsonValueKind.Number && total.TryGetInt32(out var n)
				? n
				: lista.Plantillas.Count;

			if (data.TryGetProperty("categories", out var categorias) && categorias.ValueKind == JsonValueKind.Array)
			{
				lista.Categorias = categorias.EnumerateArray()
					.Where(c => c.ValueKind == JsonValueKind.String)
					.Select(c => c.GetString()!)
					.Where(c => c.Length is > 0 and <= 30)
					.ToList();
			}

			return lista;
		}

		private static PlantillaMeme? LeerPlantilla(JsonElement e)
		{
			if (e.ValueKind != JsonValueKind.Object)
			{
				return null;
			}

			var id = Texto(e, "id");
			var imagen = Texto(e, "image_url");
			if (id.Length == 0 || id.Length > 80 || !EsImagenHttps(imagen))
			{
				return null;
			}

			var nombre = Texto(e, "name");
			return new PlantillaMeme
			{
				vchId = id,
				nvchNombre = nombre.Length > 0 ? Recortar(nombre, 80) : id,
				vchCategoria = Recortar(Texto(e, "category"), 30),
				vchImagenUrl = imagen,
				iCajas = Entero(e, "box_count"),
				iAncho = Entero(e, "width"),
				iAlto = Entero(e, "height")
			};
		}

		/// <summary>La URL viene del proveedor, pero igual se exige https, host con nombre (no IP) y que no sea local.</summary>
		public static bool EsImagenHttps(string? url)
		{
			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
			{
				return false;
			}

			if (uri.HostNameType != UriHostNameType.Dns)
			{
				return false;
			}

			var host = uri.Host.ToLowerInvariant();
			return host != "localhost" && !host.EndsWith(".local") && !host.EndsWith(".internal") && url!.Length <= 600;
		}

		private static string Texto(JsonElement e, string nombre) =>
			e.TryGetProperty(nombre, out var v) && v.ValueKind == JsonValueKind.String ? (v.GetString() ?? string.Empty).Trim() : string.Empty;

		private static int Entero(JsonElement e, string nombre) =>
			e.TryGetProperty(nombre, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n) ? Math.Max(0, n) : 0;

		private static string Recortar(string texto, int max) => texto.Length > max ? texto[..max] : texto;
	}
}
