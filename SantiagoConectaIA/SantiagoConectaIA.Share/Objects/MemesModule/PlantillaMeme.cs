namespace SantiagoConectaIA.Share.Objects.MemesModule
{
	/// <summary>Plantilla de meme de la biblioteca (createa.meme).</summary>
	public class PlantillaMeme
	{
		public string vchId { get; set; } = string.Empty;
		public string nvchNombre { get; set; } = string.Empty;
		public string vchCategoria { get; set; } = string.Empty;
		public string vchImagenUrl { get; set; } = string.Empty;

		/// <summary>Cantidad de cajas de texto que trae la plantilla.</summary>
		public int iCajas { get; set; }
		public int iAncho { get; set; }
		public int iAlto { get; set; }
	}

	public class ListaPlantillasMeme
	{
		public List<PlantillaMeme> Plantillas { get; set; } = new();
		public int iTotal { get; set; }
		public List<string> Categorias { get; set; } = new();
	}

	public static class ModosMeme
	{
		public const string Tendencias = "TENDENCIAS";
		public const string Biblioteca = "BIBLIOTECA";
	}
}
