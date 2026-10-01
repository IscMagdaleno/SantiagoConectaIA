using SantiagoConectaIA.Share.Objects.CatalogosModule;

namespace SantiagoConectaIA.API.Services
{
    public interface IParametrosService
    {
        Task<Parametro?> GetAsync(string alias, CancellationToken cancellationToken = default);
        Task<string?> GetValor1Async(string alias, CancellationToken cancellationToken = default);
        Task<string?> GetValor2Async(string alias, CancellationToken cancellationToken = default);
        Task<JwtParametros> GetJwtAsync(CancellationToken cancellationToken = default);
    }

    public sealed record JwtParametros(string? Secret, string Issuer, string Audience);
}
