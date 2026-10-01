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
                var result = await response.Content.ReadFromJsonAsync<Response<T>>();
                return result ?? Response<T>.BadResult("No se recibió respuesta del servidor.", fallback);
            }
            catch (Exception ex)
            {
                return Response<T>.BadResult(ex.Message, fallback);
            }
        }
    }
}
