using System.Net.Http.Json;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles
{
    public class MainPublicaciones
    {
        private const string Url = "api/Publicaciones";

        private readonly HttpClient _httpClient;

        public MainPublicaciones(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<Response<string>> MejorarConIa(int idNoticia, string titulo, string contenido)
        {
            var model = new PostMejorarPublicacion
            {
                iIdNoticia = idNoticia,
                vchTitulo = titulo,
                nvchContenido = contenido
            };
            return Post($"{Url}/PostMejorarPublicacion", model, string.Empty);
        }

        public Task<Response<string>> MejorarEmprendimientoConIa(int idEmpresa, string nombre, string informacion)
        {
            var model = new PostMejorarEmprendimiento
            {
                iIdEmpresa = idEmpresa,
                vchNombreComercial = nombre,
                nvchInformacion = informacion
            };
            return Post($"{Url}/PostMejorarEmprendimiento", model, string.Empty);
        }

        public Task<Response<string>> MejorarProductoConIa(int idEmpresa, string nombreComercial, ProductoServicio producto, string informacion)
        {
            var model = new PostMejorarProducto
            {
                iIdEmpresa = idEmpresa,
                iIdProducto = producto.iIdProducto,
                vchNombreComercial = nombreComercial,
                vchNombreProducto = producto.vchNombre ?? string.Empty,
                nvchInformacion = informacion
            };
            return Post($"{Url}/PostMejorarProducto", model, string.Empty);
        }

        public Task<Response<string>> MejorarEventoConIa(PostMejorarEvento evento)
        {
            return Post($"{Url}/PostMejorarEvento", evento, string.Empty);
        }

        public async Task<Response<List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>>> GetEventos()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Eventos/PostGetEventos", new SantiagoConectaIA.Share.PostClass.EventosModulo.PostGetEventos { bEstatus = true });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Response<List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>>>();
                    if (result != null) return result;
                }
                return Response<List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>>.BadResult("No se pudieron consultar los eventos.", new List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>());
            }
            catch (Exception ex)
            {
                return Response<List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>>.BadResult("Error: " + ex.Message, new List<SantiagoConectaIA.Share.Objects.EventosModulo.Evento>());
            }
        }

        public Task<Response<string>> EditarImagenConIa(string imagenUrl, string prompt)
        {
            var model = new PostEditarImagen
            {
                vchImagenUrl = imagenUrl,
                vchPrompt = prompt
            };
            return Post($"{Url}/PostEditarImagenIa", model, string.Empty);
        }

        public Task<Response<string>> GuardarImagenBase64(string base64Data, string titulo)
        {
            var model = new PostGuardarImagenGenerada
            {
                Base64Data = base64Data,
                Titulo = titulo
            };
            return Post($"{Url}/PostGuardarImagenBase64", model, string.Empty);
        }

        public Task<Response<string>> ProxyImageBase64(string imagenUrl)
        {
            var model = new PostEditarImagen
            {
                vchImagenUrl = imagenUrl
            };
            return Post($"{Url}/PostProxyImageBase64", model, string.Empty);
        }

        public Task<Response<Dictionary<string, string>>> ProxyBatchImagesBase64(List<string> urls)
        {
            var model = new PostProxyBatchImages
            {
                Urls = urls
            };
            return Post($"{Url}/PostProxyBatchImagesBase64", model, new Dictionary<string, string>());
        }

        public Task<Response<string>> PublicarFacebook(string message, string imageUrl)
        {
            var model = new PostPublicarFacebook
            {
                Message = message,
                ImageUrl = imageUrl
            };
            return Post($"{Url}/PostPublicarFacebook", model, string.Empty);
        }

        public Task<Response<PublicacionAutomatica>> GetPublicacionAutomatica()
        {
            return Post($"{Url}/PostGetPublicacionAutomatica", new { }, new PublicacionAutomatica());
        }

        public Task<Response<PublicacionAutomatica>> SavePublicacionAutomatica(bool activo)
        {
            return Post($"{Url}/PostSavePublicacionAutomatica", new PublicacionAutomatica { bActivo = activo }, new PublicacionAutomatica());
        }

        public Task<Response<PublicacionAutomaticaEmprendimientos>> GetPublicacionAutomaticaEmprendimientos()
        {
            return Post($"{Url}/PostGetPublicacionAutomaticaEmprendimientos", new { }, new PublicacionAutomaticaEmprendimientos());
        }

        public Task<Response<PublicacionAutomaticaEmprendimientos>> SavePublicacionAutomaticaEmprendimientos(bool activo)
        {
            return Post($"{Url}/PostSavePublicacionAutomaticaEmprendimientos", new PublicacionAutomaticaEmprendimientos { bActivo = activo }, new PublicacionAutomaticaEmprendimientos());
        }

        public Task<Response<SantiagoConectaIA.Share.Objects.NoticiasModule.Noticia>> GetSiguienteNoticia()
        {
            return Post($"{Url}/PostGetSiguienteNoticia", new { }, new SantiagoConectaIA.Share.Objects.NoticiasModule.Noticia());
        }

        public Task<Response<PublicacionAutomatica>> PublicarNoticiaGenerada(int idNoticia, string message, string imageUrl)
        {
            var model = new PostPublicarGenerada { iIdRegistro = idNoticia, Message = message, ImageUrl = imageUrl };
            return Post($"{Url}/PostPublicarNoticiaGenerada", model, new PublicacionAutomatica());
        }

        public Task<Response<Empresa>> GetSiguienteEmprendimiento()
        {
            return Post($"{Url}/PostGetSiguienteEmprendimiento", new { }, new Empresa());
        }

        public Task<Response<PublicacionAutomaticaEmprendimientos>> PublicarEmprendimientoGenerado(int idEmpresa, string message, string imageUrl)
        {
            var model = new PostPublicarGenerada { iIdRegistro = idEmpresa, Message = message, ImageUrl = imageUrl };
            return Post($"{Url}/PostPublicarEmprendimientoGenerado", model, new PublicacionAutomaticaEmprendimientos());
        }
        public Task<Response<List<Empresa>>> GetEmprendimientos()
        {
            return Post("api/Empresas/PostGetEmpresas", new PostGetEmpresas { bEstatus = true }, new List<Empresa>());
        }

        public Task<Response<PostSaveEmprendimientoFull>> GetEmprendimientoCompleto(Empresa empresa)
        {
            var model = new PostGetEmprendimientoFullById
            {
                iIdEmpresa = empresa.iIdEmpresa,
                iIdPropietario = empresa.iIdPropietario ?? 0
            };
            return Post("api/Empresas/PostGetEmprendimientoFullById", model, new PostSaveEmprendimientoFull());
        }

        private async Task<Response<T>> Post<TRequest, T>(string url, TRequest model, T fallback)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, model);
                var texto = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(texto))
                {
                    try
                    {
                        var result = System.Text.Json.JsonSerializer.Deserialize<Response<T>>(texto, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // La respuesta no es el formato esperado: se reporta abajo con el código HTTP.
                    }
                }

                var detalle = (int)response.StatusCode switch
                {
                    401 => "no autorizado (401)",
                    404 => "el servicio no existe en el API publicado (404): probablemente falta publicar la última versión del API",
                    405 => "método no permitido (405)",
                    >= 500 => $"error interno del servidor ({(int)response.StatusCode})",
                    _ => $"respuesta inesperada ({(int)response.StatusCode})"
                };
                return Response<T>.BadResult($"{url}: {detalle}.", fallback);
            }
            catch (Exception ex)
            {
                return Response<T>.BadResult($"{url}: {ex.Message}", fallback);
            }
        }
    }
}