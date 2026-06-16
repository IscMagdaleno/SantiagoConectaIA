using Microsoft.Extensions.Logging;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Servicios
{
    public class NoticiasScraperService : INoticiasScraperService
    {
        private readonly ILogger<NoticiasScraperService> _logger;
        private readonly INoticiasDomain _noticiasDomain;
        private readonly HttpClient _httpClient;

        public NoticiasScraperService(ILogger<NoticiasScraperService> logger, INoticiasDomain noticiasDomain)
        {
            _logger = logger;
            _noticiasDomain = noticiasDomain;
            _httpClient = new HttpClient();
        }

        public async Task RunScrapingAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando la extracción de noticias mediante API JSON de Santiago Papasquiaro...");

            try
            {
                var apiUrl = "https://lisa-ia.com/api/feed/3/notes?categoria_id=12&page=1&geografia_id=6";

                var responseJson = await _httpClient.GetStringAsync(apiUrl, cancellationToken);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseJson, options);

                if (apiResponse?.Data == null || !apiResponse.Data.Any())
                {
                    _logger.LogWarning("La API no devolvió datos en el endpoint geográfico.");
                    return;
                }

                // === PREVENIR DUPLICADOS ===
                // Obtener las noticias que ya existen en la base de datos
                var currentNewsResponse = await _noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = null });
                var existingTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (currentNewsResponse.IsSuccess && currentNewsResponse.Data != null)
                {
                    foreach (var n in currentNewsResponse.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(n.vchTitulo))
                        {
                            existingTitles.Add(n.vchTitulo.Trim());
                        }
                    }
                }

                foreach (var nota in apiResponse.Data)
                {
                    try
                    {
                        var title = nota.Destino_Encabezado?.Trim() ?? "";
                        var contentHtml = nota.Destino_Contenido ?? "";
                        var imageUrl = nota.Img_Principal_Url ?? "";
                        
                        // Comprobar si el título ya existe
                        if (existingTitles.Contains(title))
                        {
                            _logger.LogInformation($"La noticia ya existe en la BD, saltando: {title}");
                            continue;
                        }

                        _logger.LogInformation($"Procesando Noticia Nueva (Geo: {nota.Destino_Geografia_Nombre}): {title}");

                        // Extraer un resumen sin HTML para nvchContenido
                        var plainText = ExtractPlainText(contentHtml);
                        var summary = plainText.Length > 250 ? plainText.Substring(0, 247) + "..." : plainText;

                        // Construir el modelo para Domain
                        var postModel = new PostSaveNoticia
                        {
                            iIdNoticia = 0,
                            vchTitulo = title,
                            vchTituloEn = "",
                            nvchContenido = summary,
                            nvchContenidoEn = "",
                            vchImagenPortada = imageUrl,
                            dtFechaPublicacion = DateTime.Now,
                            bActivo = true,
                            iIdCategoria = null,
                            Filas = new List<NoticiaFila>
                            {
                                new NoticiaFila
                                {
                                    iIdFila = 0,
                                    iOrden = 1,
                                    Metadatos = new List<NoticiaMetadato>
                                    {
                                        new NoticiaMetadato
                                        {
                                            iIdMetadato = 0,
                                            iIdTipoDato = TipoDatoMetadato.Contenido,
                                            vchTitulo = "Contenido Principal",
                                            nvchValor = contentHtml, // HTML completo
                                            iOrden = 1,
                                            iAncho = 12,
                                            vchAlineacion = "none",
                                            vchAlto = "auto",
                                            nvchConfiguracion = "{}"
                                        }
                                    }
                                }
                            }
                        };

                        // Guardar noticia
                        var saveResult = await _noticiasDomain.SaveNoticia(postModel);

                        if (saveResult.IsSuccess)
                        {
                            _logger.LogInformation($"Noticia guardada con éxito. ID: {saveResult.Data.iIdNoticia}");
                        }
                        else
                        {
                            _logger.LogError($"Error al guardar noticia {title}: {saveResult.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar una nota individual de la API.");
                    }
                }

                _logger.LogInformation("Proceso de extracción finalizado con éxito.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error general al consultar la API.");
            }
        }

        private string ExtractPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText).Trim();
        }

        // Clases para mapear el JSON específico de la API de Orale Que Chiquito
        public class ApiResponse
        {
            public NotaData[] Data { get; set; }
        }

        public class NotaData
        {
            public string Destino_Encabezado { get; set; }
            public string Destino_Contenido { get; set; }
            public string Img_Principal_Url { get; set; }
            public string Destino_Fecha_Publicacion { get; set; }
            public string Destino_Geografia_Nombre { get; set; }
        }
    }
}
