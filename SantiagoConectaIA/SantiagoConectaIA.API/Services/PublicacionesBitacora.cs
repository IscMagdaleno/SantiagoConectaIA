using System.Collections.Concurrent;

namespace SantiagoConectaIA.API.Services
{
    /// <summary>
    /// Memoria de las publicaciones automáticas: cuándo quedó programada cada una y cómo terminó el último intento.
    /// Sirve para diagnosticar producción sin abrir los logs del servidor (se reinicia con el proceso).
    /// </summary>
    public sealed class PublicacionesBitacora
    {
        public const string Noticias = "noticias";
        public const string Emprendimientos = "emprendimientos";

        private readonly ConcurrentDictionary<string, DateTime> _programadas = new();
        private readonly ConcurrentDictionary<string, (DateTime Utc, string Resultado)> _intentos = new();

        public DateTime InicioUtc { get; } = DateTime.UtcNow;

        public void Programar(string proceso, DateTime ejecucionUtc) => _programadas[proceso] = ejecucionUtc;

        public void Registrar(string proceso, string resultado) => _intentos[proceso] = (DateTime.UtcNow, resultado);

        public DateTime? ProximaEjecucionUtc(string proceso) => _programadas.TryGetValue(proceso, out var fecha) ? fecha : null;

        public (DateTime Utc, string Resultado)? UltimoIntento(string proceso) => _intentos.TryGetValue(proceso, out var intento) ? intento : null;
    }
}
