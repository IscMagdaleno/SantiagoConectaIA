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

        public static TimeSpan TiempoHasta(int hora)
        {
            var zona = ZonaHoraria();
            var ahora = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zona);
            var proxima = ahora.Date.AddHours(hora);
            if (ahora >= proxima)
            {
                proxima = proxima.AddDays(1);
            }

            var proximaUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(proxima, DateTimeKind.Unspecified), zona);
            var espera = proximaUtc - DateTime.UtcNow;
            return espera < TimeSpan.Zero ? TimeSpan.Zero : espera;
        }
    }
}
