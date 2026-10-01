using System.Text.Json;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public class PublicacionAutomaticaService : IPublicacionAutomaticaService
    {
        private const string ArchivoNoticias = "publicacion-automatica.json";
        private const string ArchivoEmprendimientos = "publicacion-automatica-emprendimientos.json";

        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly IHostEnvironment _environment;
        private readonly IParametrosService _parametros;

        public PublicacionAutomaticaService(IHostEnvironment environment, IParametrosService parametros)
        {
            _environment = environment;
            _parametros = parametros;
        }

        public async Task<int> HoraPublicacionAsync(CancellationToken cancellationToken = default)
        {
            var valor = await _parametros.GetValor1Async(ParametrosAlias.PublicacionesAutoNoticias, cancellationToken);
            return Hora(valor, 11);
        }

        public async Task<int> HoraEmprendimientosAsync(CancellationToken cancellationToken = default)
        {
            var valor = await _parametros.GetValor1Async(ParametrosAlias.PublicacionesAutoEmprendimientos, cancellationToken);
            return Hora(valor, 13);
        }

        public async Task<PublicacionAutomatica> GetAsync(CancellationToken cancellationToken = default)
        {
            var estado = await ConLockAsync(() => LeerAsync(ArchivoNoticias, () => new PublicacionAutomatica(), cancellationToken), cancellationToken);
            estado.iHora = await HoraPublicacionAsync(cancellationToken);
            return estado;
        }

        public async Task<PublicacionAutomatica> SetActivoAsync(bool activo, CancellationToken cancellationToken = default)
        {
            var estado = await ActualizarAsync(ArchivoNoticias, () => new PublicacionAutomatica(), e => e.bActivo = activo, cancellationToken);
            estado.iHora = await HoraPublicacionAsync(cancellationToken);
            return estado;
        }

        public Task RegistrarPublicacionAsync(int idNoticia, string titulo, CancellationToken cancellationToken = default)
        {
            return ActualizarAsync(ArchivoNoticias, () => new PublicacionAutomatica(), e =>
            {
                e.iIdUltimaNoticia = idNoticia;
                e.vchUltimaNoticia = titulo;
                e.dtUltimaPublicacion = DateTime.UtcNow;
            }, cancellationToken);
        }

        public async Task<PublicacionAutomaticaEmprendimientos> GetEmprendimientosAsync(CancellationToken cancellationToken = default)
        {
            var nuevo = await NuevoEstadoEmprendimientosAsync(cancellationToken);
            var estado = await ConLockAsync(() => LeerAsync(ArchivoEmprendimientos, nuevo, cancellationToken), cancellationToken);
            estado.iHora = await HoraEmprendimientosAsync(cancellationToken);
            return estado;
        }

        public async Task<PublicacionAutomaticaEmprendimientos> SetEmprendimientosActivoAsync(bool activo, CancellationToken cancellationToken = default)
        {
            var nuevo = await NuevoEstadoEmprendimientosAsync(cancellationToken);
            var estado = await ActualizarAsync(ArchivoEmprendimientos, nuevo, e => e.bActivo = activo, cancellationToken);
            estado.iHora = await HoraEmprendimientosAsync(cancellationToken);
            return estado;
        }

        public async Task RegistrarEmprendimientoAsync(int idEmpresa, string nombre, CancellationToken cancellationToken = default)
        {
            var nuevo = await NuevoEstadoEmprendimientosAsync(cancellationToken);
            await ActualizarAsync(ArchivoEmprendimientos, nuevo, e =>
            {
                e.iIdUltimaEmpresa = idEmpresa;
                e.vchUltimaEmpresa = nombre;
                e.dtUltimaPublicacion = DateTime.UtcNow;
                e.iIdSiguienteEmpresa = idEmpresa + 1;
            }, cancellationToken);
        }

        private async Task<Func<PublicacionAutomaticaEmprendimientos>> NuevoEstadoEmprendimientosAsync(CancellationToken cancellationToken)
        {
            var valor = await _parametros.GetValor2Async(ParametrosAlias.PublicacionesAutoEmprendimientos, cancellationToken);
            var inicial = int.TryParse(valor, out var id) && id > 0 ? id : 1;
            return () => new PublicacionAutomaticaEmprendimientos { iIdSiguienteEmpresa = inicial };
        }

        private static int Hora(string? valor, int defecto)
        {
            return int.TryParse(valor, out var hora) && hora is >= 0 and <= 23 ? hora : defecto;
        }

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
