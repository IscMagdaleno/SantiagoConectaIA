using SantiagoConectaIA.Share.Objects.MemesModule;

namespace SantiagoConectaIA.API.Services
{
	public sealed class PlantillasResultado
	{
		public bool Exitosa { get; init; }
		public string Mensaje { get; init; } = string.Empty;
		public ListaPlantillasMeme Lista { get; init; } = new();

		public static PlantillasResultado Falla(string mensaje) => new() { Exitosa = false, Mensaje = mensaje };
	}

	/// <summary>
	/// Biblioteca de plantillas de meme (createa.meme). Solo se usan los endpoints GRATIS de plantillas y tendencias:
	/// la imagen del meme la genera Gemini, así que no se consumen créditos de generación del proveedor.
	/// </summary>
	public interface IMemePlantillasClient
	{
		Task<PlantillasResultado> ListarAsync(string modo, string? busqueda, string? categoria, string periodo, int limite, int desde, CancellationToken cancellationToken = default);

		/// <summary>Busca una plantilla por id. Solo devuelve plantillas que el propio proveedor publicó (nunca URLs del cliente).</summary>
		Task<PlantillaMeme?> ObtenerAsync(string id, CancellationToken cancellationToken = default);
	}
}
