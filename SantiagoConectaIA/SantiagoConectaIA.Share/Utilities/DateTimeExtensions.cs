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

		private static readonly System.Globalization.CultureInfo EsMx = new("es-MX");

		/// <summary>
		/// Tiempo transcurrido en español ("Justo ahora", "Hace 5 min", "Hace 3 h", "Hace 2 d" o la fecha).
		/// Una fecha sin zona horaria se asume UTC, igual que en <see cref="ToUsuarioLocal(DateTime)"/>.
		/// </summary>
		public static string ToTiempoRelativo(this DateTime dt)
		{
			if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
				return string.Empty;

			var utc = dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
			var diff = DateTime.UtcNow - utc;

			if (diff.TotalSeconds < 60) return "Justo ahora";
			if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
			if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours} h";
			if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays} d";

			return utc.ToLocalTime().ToString("d MMM yyyy", EsMx);
		}
	}
}
