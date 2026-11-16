namespace SantiagoConectaIA.Share.Objects.MemesModule
{
	/// <summary>Un meme creado con IA (guardado en el historial).</summary>
	public class MemeCreado
	{
		public int iIdMeme { get; set; }
		public string vchPlantillaId { get; set; } = string.Empty;
		public string nvchPlantillaNombre { get; set; } = string.Empty;
		public string vchPlantillaUrl { get; set; } = string.Empty;
		public string nvchIdea { get; set; } = string.Empty;
		public string vchResultadoUrl { get; set; } = string.Empty;

		/// <summary>Fecha y hora en UTC.</summary>
		public DateTime dtCreacion { get; set; }
	}

	/// <summary>Resultado de crear un meme: el meme y cuánto se ha usado hoy.</summary>
	public class ResultadoMeme
	{
		public MemeCreado Meme { get; set; } = new();
		public int iUsoHoy { get; set; }
		public int iLimiteDiario { get; set; }
	}

	/// <summary>Uso de la IA para memes en el día (hora de México).</summary>
	public class ResumenMemes
	{
		public int iUsoHoy { get; set; }
		public int iLimiteDiario { get; set; }
	}
}
