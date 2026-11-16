using System.Globalization;

namespace SantiagoConectaIA.API.Services
{
	public static class HorarioPublicacion
	{
		public static TimeZoneInfo ZonaHoraria()
		{
			foreach (var id in new[] { "America/Mexico_City", "Central Standard Time (Mexico)", "Central Standard Time" })
			{
				if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zona))
				{
					return zona;
				}
			}

			return TimeZoneInfo.Local;
		}

		public static DateTime FechaLocal(DateTime utc)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ZonaHoraria()).Date;
		}

		public static DateTime HoyLocal() => FechaLocal(DateTime.UtcNow);

		/// <summary>
		/// Lee la hora de un parámetro. Acepta "HH:mm" (por ejemplo "12:30") o una hora sola ("13").
		/// Si el valor no es válido devuelve la hora por defecto.
		/// </summary>
		public static TimeSpan LeerHora(string? valor, TimeSpan porDefecto)
		{
			valor = valor?.Trim();
			if (string.IsNullOrEmpty(valor))
			{
				return porDefecto;
			}

			if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var soloHora))
			{
				return soloHora is >= 0 and <= 23 ? TimeSpan.FromHours(soloHora) : porDefecto;
			}

			if (TimeSpan.TryParseExact(valor, new[] { @"h\:mm", @"hh\:mm" }, CultureInfo.InvariantCulture, out var hora)
				&& hora >= TimeSpan.Zero && hora < TimeSpan.FromDays(1))
			{
				return hora;
			}

			return porDefecto;
		}

		public static string Formato(TimeSpan hora) => hora.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

		/// <summary>True si en México ya pasó la hora indicada del día en curso.</summary>
		public static bool YaPasoLaHora(TimeSpan hora)
		{
			var ahora = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ZonaHoraria());
			return ahora.TimeOfDay >= hora;
		}

		/// <summary>Próxima vez (UTC) que será esa hora en México; si hoy ya pasó, mañana.</summary>
		public static DateTime ProximaUtc(TimeSpan hora)
		{
			var zona = ZonaHoraria();
			var ahora = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zona);
			var proxima = ahora.Date.Add(hora);
			if (ahora.DateTime >= proxima)
			{
				proxima = proxima.AddDays(1);
			}

			return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(proxima, DateTimeKind.Unspecified), zona);
		}

		public static TimeSpan TiempoHasta(TimeSpan hora)
		{
			var espera = ProximaUtc(hora) - DateTime.UtcNow;
			return espera < TimeSpan.Zero ? TimeSpan.Zero : espera;
		}
	}
}
