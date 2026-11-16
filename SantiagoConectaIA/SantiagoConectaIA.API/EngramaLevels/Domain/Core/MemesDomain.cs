using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.MemesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class MemesDomain : IMemesDomain
	{
		private readonly IMemesRepository _repository;

		public MemesDomain(IMemesRepository repository)
		{
			_repository = repository;
		}

		public async Task<Response<int>> GuardarMeme(MemeCreado meme, int idUsuario)
		{
			try
			{
				var result = await _repository.spSaveMemeCreado(new spSaveMemeCreado.Request
				{
					vchPlantillaId = meme.vchPlantillaId,
					nvchPlantillaNombre = Recortar(meme.nvchPlantillaNombre, 120),
					vchPlantillaUrl = Recortar(meme.vchPlantillaUrl, 600),
					nvchIdea = Recortar(meme.nvchIdea, 500),
					vchResultadoUrl = Recortar(meme.vchResultadoUrl, 1000),
					iIdUsuario = idUsuario > 0 ? idUsuario : null
				});

				return result.bResult
					? new Response<int> { IsSuccess = true, Data = result.iIdMeme, Message = "Ok" }
					: Response<int>.BadResult(string.IsNullOrWhiteSpace(result.vchMessage) ? "No se pudo guardar el meme." : result.vchMessage, 0);
			}
			catch (Exception ex)
			{
				return Response<int>.BadResult(ex.Message, 0);
			}
		}

		public async Task<Response<List<MemeCreado>>> GetRecientes(int top)
		{
			try
			{
				var filas = (await _repository.spGetMemesCreados(new spGetMemesCreados.Request { iTop = top > 0 ? top : 12 })).ToList();

				// Si el procedimiento falló, el repositorio devuelve una sola fila de error sin identificador.
				var primera = filas.FirstOrDefault();
				if (primera != null && !primera.bResult && primera.iIdMeme == 0)
				{
					return Response<List<MemeCreado>>.BadResult(
						string.IsNullOrWhiteSpace(primera.vchMessage) ? "No se pudo consultar el historial de memes." : primera.vchMessage,
						new List<MemeCreado>());
				}

				var datos = filas.Select(f => new MemeCreado
				{
					iIdMeme = f.iIdMeme,
					vchPlantillaId = f.vchPlantillaId ?? string.Empty,
					nvchPlantillaNombre = f.nvchPlantillaNombre ?? string.Empty,
					vchPlantillaUrl = f.vchPlantillaUrl ?? string.Empty,
					nvchIdea = f.nvchIdea ?? string.Empty,
					vchResultadoUrl = f.vchResultadoUrl ?? string.Empty,
					// La base guarda UTC; se marca como tal para que no se convierta dos veces.
					dtCreacion = DateTime.SpecifyKind(f.dtCreacion, DateTimeKind.Utc)
				}).ToList();

				return new Response<List<MemeCreado>> { IsSuccess = true, Data = datos, Message = "Ok" };
			}
			catch (Exception ex)
			{
				return Response<List<MemeCreado>>.BadResult(ex.Message, new List<MemeCreado>());
			}
		}

		public async Task<Response<int>> GetUsoHoy()
		{
			try
			{
				var result = await _repository.spGetMemesResumen(new spGetMemesResumen.Request());
				return result.bResult
					? new Response<int> { IsSuccess = true, Data = result.iUsoHoy, Message = "Ok" }
					: Response<int>.BadResult(string.IsNullOrWhiteSpace(result.vchMessage) ? "No se pudo consultar el uso de hoy." : result.vchMessage, 0);
			}
			catch (Exception ex)
			{
				return Response<int>.BadResult(ex.Message, 0);
			}
		}

		public async Task<Response<bool>> Descartar(int idMeme)
		{
			try
			{
				if (idMeme <= 0) return Response<bool>.BadResult("Indica el meme que quieres quitar.", false);

				var result = await _repository.spDescartarMeme(new spDescartarMeme.Request { iIdMeme = idMeme });
				return result.bResult
					? new Response<bool> { IsSuccess = true, Data = true, Message = "Ok" }
					: Response<bool>.BadResult(string.IsNullOrWhiteSpace(result.vchMessage) ? "No se pudo quitar el meme." : result.vchMessage, false);
			}
			catch (Exception ex)
			{
				return Response<bool>.BadResult(ex.Message, false);
			}
		}

		private static string Recortar(string? texto, int max)
		{
			texto ??= string.Empty;
			return texto.Length > max ? texto[..max] : texto;
		}
	}
}
