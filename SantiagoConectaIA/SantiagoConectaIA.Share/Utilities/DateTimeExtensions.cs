using System;

namespace SantiagoConectaIA.Share.Utilities
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convierte una fecha almacenada en UTC o sin zona horaria a la hora local del usuario/dispositivo.
        /// Si la fecha es DateTimeKind.Unspecified, la asume como UTC antes de convertir a LocalTime.
        /// </summary>
        public static DateTime ToUsuarioLocal(this DateTime dt)
        {
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
                return dt;

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                // La fecha vino de la base de datos sin especificar zona horaria, pero representa un instante UTC
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            }

            return dt.ToLocalTime();
        }

        /// <summary>
        /// Convierte una fecha nullable a hora local.
        /// </summary>
        public static DateTime? ToUsuarioLocal(this DateTime? dt)
        {
            if (!dt.HasValue)
                return null;

            return dt.Value.ToUsuarioLocal();
        }
    }
}
