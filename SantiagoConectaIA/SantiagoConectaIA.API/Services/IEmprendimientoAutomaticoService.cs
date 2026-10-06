using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;

namespace SantiagoConectaIA.API.Services
{
    public interface IEmprendimientoAutomaticoService
    {
        Task<Empresa?> GetSiguienteAsync(int idSiguiente, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publica en Facebook el siguiente emprendimiento de la rotación. Con manual = true ignora el
        /// interruptor y el límite de una publicación por día (lo usa el botón "Publicar siguiente").
        /// </summary>
        Task<Response<string>> PublicarSiguienteAsync(bool manual = false, CancellationToken cancellationToken = default);
    }
}
