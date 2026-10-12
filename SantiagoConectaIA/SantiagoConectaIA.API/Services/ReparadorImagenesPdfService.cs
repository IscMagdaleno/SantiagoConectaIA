using Microsoft.EntityFrameworkCore;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.DAL.Models;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Solicitud de reparación. Por seguridad la simulación está activada por defecto.
	/// </summary>
	public sealed class PostRepararImagenesPdf
	{
		/// <summary>true = solo reporta qué cambiaría (no sube archivos ni modifica la base de datos).</summary>
		public bool bSimular { get; set; } = true;
	}

	/// <summary>Registro de la base de datos cuya imagen está guardada con extensión .pdf.</summary>
	public sealed class ImagenPdfRegistro
	{
		public string Tipo { get; set; } = string.Empty;
		public int Id { get; set; }
		public string Url { get; set; } = string.Empty;
	}

	/// <summary>Resultado por registro. Nunca incluye la firma SAS de las URLs.</summary>
	public sealed class ImagenPdfResultado
	{
		public string Tipo { get; set; } = string.Empty;
		public int Id { get; set; }
		public string ArchivoActual { get; set; } = string.Empty;
		public string FormatoReal { get; set; } = string.Empty;
		public string ArchivoNuevo { get; set; } = string.Empty;
		public string Estado { get; set; } = string.Empty;
		public string Detalle { get; set; } = string.Empty;
	}

	/// <summary>
	/// Repara imágenes (logos de empresas e imágenes de productos) que se guardaron con extensión .pdf
	/// por un error de versiones anteriores de EngramaCoreStandar. Detecta el formato real por los bytes,
	/// vuelve a subir la imagen con su extensión y Content-Type correctos y actualiza la referencia.
	/// No borra el archivo anterior: queda como respaldo para poder revertir.
	/// </summary>
	public sealed class ReparadorImagenesPdfService
	{
		private const int MaxBytes = 10 * 1024 * 1024;
		// Ambas columnas (Empresa.vchLogoUrl y ProductoServicio.vchImagenUrl) son varchar(255).
		private const int MaxLongitudUrl = 255;
		private const string Contenedor = "empresas";

		private const string ConsultaRegistros = @"
SELECT CAST('Logo' AS NVARCHAR(20)) AS Tipo, E.iIdEmpresa AS Id, CAST(E.vchLogoUrl AS NVARCHAR(500)) AS Url
FROM SCIA.Empresa E
WHERE LEFT(E.vchLogoUrl, CHARINDEX('?', E.vchLogoUrl + '?') - 1) LIKE '%.pdf'
UNION ALL
SELECT CAST('Producto' AS NVARCHAR(20)), P.iIdProducto, CAST(P.vchImagenUrl AS NVARCHAR(500))
FROM SCIA.ProductoServicio P
WHERE LEFT(P.vchImagenUrl, CHARINDEX('?', P.vchImagenUrl + '?') - 1) LIKE '%.pdf'";

		private readonly EngramaContext _context;
		private readonly IAzureBlobDomain _blobDomain;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ReparadorImagenesPdfService> _logger;

		public ReparadorImagenesPdfService(
			EngramaContext context,
			IAzureBlobDomain blobDomain,
			IHttpClientFactory httpClientFactory,
			ILogger<ReparadorImagenesPdfService> logger)
		{
			_context = context;
			_blobDomain = blobDomain;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public async Task<List<ImagenPdfResultado>> EjecutarAsync(bool simular, CancellationToken cancellationToken)
		{
			var registros = await _context.Database
				.SqlQueryRaw<ImagenPdfRegistro>(ConsultaRegistros)
				.ToListAsync(cancellationToken);

			var cliente = _httpClientFactory.CreateClient();
			cliente.Timeout = TimeSpan.FromSeconds(60);

			// Si varios registros apuntan al mismo archivo, se sube una sola vez.
			var yaSubidos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var resultados = new List<ImagenPdfResultado>();

			foreach (var registro in registros)
			{
				var resultado = new ImagenPdfResultado
				{
					Tipo = registro.Tipo,
					Id = registro.Id,
					ArchivoActual = SinFirma(registro.Url)
				};
				resultados.Add(resultado);

				try
				{
					await ProcesarAsync(registro, resultado, cliente, yaSubidos, simular, cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "No se pudo reparar la imagen {Tipo} {Id}", registro.Tipo, registro.Id);
					resultado.Estado = "Error";
					resultado.Detalle = ex.Message;
				}
			}

			return resultados;
		}

		private async Task ProcesarAsync(
			ImagenPdfRegistro registro,
			ImagenPdfResultado resultado,
			HttpClient cliente,
			Dictionary<string, string> yaSubidos,
			bool simular,
			CancellationToken cancellationToken)
		{
			if (!yaSubidos.TryGetValue(resultado.ArchivoActual, out var urlNueva))
			{
				using var respuesta = await cliente.GetAsync(registro.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
				if (!respuesta.IsSuccessStatusCode)
				{
					resultado.Estado = "Omitido";
					resultado.Detalle = $"No se pudo descargar el archivo (HTTP {(int)respuesta.StatusCode}).";
					return;
				}

				if (respuesta.Content.Headers.ContentLength > MaxBytes)
				{
					resultado.Estado = "Omitido";
					resultado.Detalle = "El archivo supera 10 MB.";
					return;
				}

				var bytes = await respuesta.Content.ReadAsByteArrayAsync(cancellationToken);
				var formato = FormatoArchivo.Detectar(bytes);
				resultado.FormatoReal = formato?.Extension.ToUpperInvariant() ?? "DESCONOCIDO";

				if (formato is not { EsImagen: true })
				{
					resultado.Estado = "Omitido";
					resultado.Detalle = "No es una imagen (es un PDF real u otro formato); se deja como está.";
					return;
				}

				var nombreNuevo = $"{Guid.NewGuid()}.{formato.Extension}";
				resultado.ArchivoNuevo = $"{Contenedor}/{nombreNuevo}";

				if (simular)
				{
					resultado.Estado = "Pendiente (simulación)";
					resultado.Detalle = "Se subiría con su extensión y Content-Type correctos y se actualizaría el registro.";
					return;
				}

				using var flujo = new MemoryStream(bytes);
				var subida = await _blobDomain.UploadDocument(flujo, nombreNuevo, Contenedor);
				if (!subida.IsSuccess || string.IsNullOrWhiteSpace(subida.Data?.URL))
				{
					resultado.Estado = "Error";
					resultado.Detalle = subida.Message ?? "No se pudo subir la imagen a Azure Blob.";
					return;
				}

				if (subida.Data.URL.Length > MaxLongitudUrl)
				{
					resultado.Estado = "Error";
					resultado.Detalle = $"La URL nueva mide {subida.Data.URL.Length} caracteres y la columna admite {MaxLongitudUrl}.";
					return;
				}

				urlNueva = subida.Data.URL;
				yaSubidos[resultado.ArchivoActual] = urlNueva;
			}
			else
			{
				resultado.ArchivoNuevo = SinFirma(urlNueva).Replace("https://", string.Empty);
				resultado.FormatoReal = "(mismo archivo ya reparado)";
			}

			// El UPDATE exige que la URL siga siendo la que leímos: si alguien la cambió entre tanto, no la pisa.
			var filas = registro.Tipo == "Logo"
				? await _context.Database.ExecuteSqlInterpolatedAsync(
					$"UPDATE SCIA.Empresa SET vchLogoUrl = {urlNueva} WHERE iIdEmpresa = {registro.Id} AND vchLogoUrl = {registro.Url}",
					cancellationToken)
				: await _context.Database.ExecuteSqlInterpolatedAsync(
					$"UPDATE SCIA.ProductoServicio SET vchImagenUrl = {urlNueva} WHERE iIdProducto = {registro.Id} AND vchImagenUrl = {registro.Url}",
					cancellationToken);

			resultado.Estado = filas > 0 ? "Reparado" : "Sin cambios";
			resultado.Detalle = filas > 0
				? "Imagen subida con la extensión correcta y referencia actualizada. El archivo anterior se conserva como respaldo."
				: "El registro cambió mientras se procesaba; no se modificó.";
		}

		private static string SinFirma(string url)
		{
			var indice = url.IndexOf('?');
			return indice >= 0 ? url[..indice] : url;
		}
	}
}
