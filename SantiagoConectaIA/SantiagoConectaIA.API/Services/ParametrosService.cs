using System.Collections.Concurrent;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.CatalogosModule;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;

namespace SantiagoConectaIA.API.Services
{
    public class ParametrosService : IParametrosService
    {
        private const string JwtDefault = "SantiagoConectaIA";
        private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<string, (Parametro Parametro, DateTime Expira)> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ParametrosService> _logger;

        public ParametrosService(IServiceScopeFactory scopeFactory, ILogger<ParametrosService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<Parametro?> GetAsync(string alias, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(alias, out var cacheado) && cacheado.Expira > DateTime.UtcNow)
            {
                return cacheado.Parametro;
            }

            using var scope = _scopeFactory.CreateScope();
            var catalogos = scope.ServiceProvider.GetRequiredService<ICatalogosDomain>();
            var res = await catalogos.GetParametroByAlias(new PostGetParametro { vchAlias = alias });

            if (res == null || !res.IsSuccess || res.Data == null || !res.Data.BHabilitado)
            {
                _logger.LogWarning("No se encontró el parámetro habilitado '{Alias}'.", alias);
                return null;
            }

            _cache[alias] = (res.Data, DateTime.UtcNow.Add(Vigencia));
            return res.Data;
        }

        public async Task<string?> GetValor1Async(string alias, CancellationToken cancellationToken = default)
        {
            return Limpiar((await GetAsync(alias, cancellationToken))?.NvchValor1);
        }

        public async Task<string?> GetValor2Async(string alias, CancellationToken cancellationToken = default)
        {
            return Limpiar((await GetAsync(alias, cancellationToken))?.NvchValor2);
        }

        public async Task<JwtParametros> GetJwtAsync(CancellationToken cancellationToken = default)
        {
            var secret = await GetValor1Async(ParametrosAlias.JwtSecret, cancellationToken);
            var config = await GetAsync(ParametrosAlias.JwtConfig, cancellationToken);
            return new JwtParametros(
                secret,
                Limpiar(config?.NvchValor1) ?? JwtDefault,
                Limpiar(config?.NvchValor2) ?? JwtDefault);
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
