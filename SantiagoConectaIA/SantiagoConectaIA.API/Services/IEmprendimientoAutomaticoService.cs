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

        /// <summary>
        /// Devuelve el emprendimiento que toca publicar (a partir del ID donde se quedó la rotación) validando
        /// que no se acabe de publicar y que tenga logo. El navegador lo usa para preparar el post.
        /// </summary>
        Task<Response<Empresa>> ObtenerSiguienteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Publica un emprendimiento con el texto y la imagen ya preparados (post capturado). Solo acepta el que
        /// devuelve ObtenerSiguienteAsync y avanza la rotación al que sigue.
        /// </summary>
        Task<Response<string>> PublicarGeneradoAsync(int idEmpresa, string mensaje, string imagenUrl, CancellationToken cancellationToken = default);
    }
}
