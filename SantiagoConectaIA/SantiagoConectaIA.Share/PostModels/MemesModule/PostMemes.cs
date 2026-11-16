namespace SantiagoConectaIA.Share.PostModels.MemesModule
{
	/// <summary>Consulta de plantillas: tendencias o biblioteca (con búsqueda y categoría).</summary>
	public class PostGetPlantillasMeme
	{
		/// <summary>TENDENCIAS o BIBLIOTECA.</summary>
		public string vchModo { get; set; } = "TENDENCIAS";

		/// <summary>Solo tendencias: 24h, 7d o 30d.</summary>
		public string vchPeriodo { get; set; } = "7d";

		public string? vchBusqueda { get; set; }
		public string? vchCategoria { get; set; }
		public int iLimite { get; set; } = 24;
		public int iDesde { get; set; }
	}

	/// <summary>Crea un meme: plantilla elegida + idea. La IA escribe los textos en español.</summary>
	public class PostCrearMeme
	{
		public string vchPlantillaId { get; set; } = string.Empty;
		public string vchIdea { get; set; } = string.Empty;
	}

	public class PostGetMemesRecientes
	{
		public int iTop { get; set; } = 12;
	}

	public class PostDescartarMeme
	{
		public int iIdMeme { get; set; }
	}
}
