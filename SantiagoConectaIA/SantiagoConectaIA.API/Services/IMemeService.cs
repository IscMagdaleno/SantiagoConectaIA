using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.MemesModule;
using SantiagoConectaIA.Share.PostModels.MemesModule;

namespace SantiagoConectaIA.API.Services
{
	public interface IMemeService
	{
		Task<Response<ListaPlantillasMeme>> GetPlantillasAsync(PostGetPlantillasMeme post, CancellationToken cancellationToken = default);

		/// <summary>Crea un meme con IA a partir de una plantilla del proveedor y una idea.</summary>
		Task<Response<ResultadoMeme>> CrearAsync(int idUsuario, PostCrearMeme post, CancellationToken cancellationToken = default);

		Task<Response<ResumenMemes>> GetResumenAsync(CancellationToken cancellationToken = default);
	}
}
