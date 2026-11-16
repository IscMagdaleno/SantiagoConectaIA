namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
	public class PostGetPublicacionesRegistro
	{
		/// <summary>NOTICIA o EMPRENDIMIENTO. Nulo = todos.</summary>
		public string? vchTipo { get; set; }

		/// <summary>Nulo = exitosas y fallidas.</summary>
		public bool? bExitoso { get; set; }

		/// <summary>Día local (México), inclusive.</summary>
		public DateTime? dtDesde { get; set; }

		/// <summary>Día local (México), inclusive.</summary>
		public DateTime? dtHasta { get; set; }

		public int iTop { get; set; } = 200;
	}
}
