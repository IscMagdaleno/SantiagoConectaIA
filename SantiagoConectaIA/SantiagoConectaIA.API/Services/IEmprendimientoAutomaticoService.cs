using SantiagoConectaIA.Share.Objects.EmpresasModulo;

namespace SantiagoConectaIA.API.Services
{
    public interface IEmprendimientoAutomaticoService
    {
        Task<Empresa?> GetSiguienteAsync(int idSiguiente, CancellationToken cancellationToken = default);
        Task PublicarSiguienteAsync(CancellationToken cancellationToken = default);
    }
}
