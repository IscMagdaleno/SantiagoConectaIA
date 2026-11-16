using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.MemesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IMemesDomain
	{
		/// <summary>Registra un meme creado y devuelve su identificador.</summary>
		Task<Response<int>> GuardarMeme(MemeCreado meme, int idUsuario);

		/// <summary>Memes recientes sin los descartados (compartidos por todos los administradores).</summary>
		Task<Response<List<MemeCreado>>> GetRecientes(int top);

		/// <summary>Cuántos memes se crearon hoy (hora de México).</summary>
		Task<Response<int>> GetUsoHoy();

		/// <summary>Quita un meme del historial.</summary>
		Task<Response<bool>> Descartar(int idMeme);
	}
}
