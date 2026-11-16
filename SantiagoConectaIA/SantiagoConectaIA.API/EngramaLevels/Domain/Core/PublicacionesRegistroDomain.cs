using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesRegistroModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.PublicacionesModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class PublicacionesRegistroDomain : IPublicacionesRegistroDomain
	{
		private readonly IPublicacionesRegistroRepository _repository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public PublicacionesRegistroDomain(IPublicacionesRegistroRepository repository, MapperHelper mapperHelper, IResponseHelper responseHelper)
		{
			_repository = repository;
			_mapperHelper = mapperHelper;
			_responseHelper = responseHelper;
		}

		public async Task<Response<PublicacionRegistro>> SavePublicacionRegistro(PostSavePublicacionRegistro postModel)
		{
			try
			{
				var request = _mapperHelper.Get<PostSavePublicacionRegistro, spSavePublicacionRegistro.Request>(postModel);
				var result = await _repository.spSavePublicacionRegistro(request);
				var validation = _responseHelper.Validacion<spSavePublicacionRegistro.Result, PublicacionRegistro>(result);

				if (validation.IsSuccess)
				{
					var guardado = _mapperHelper.Get<PostSavePublicacionRegistro, PublicacionRegistro>(postModel);
					guardado.iIdPublicacionRegistro = validation.Data.iIdPublicacionRegistro;
					validation.Data = guardado;
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<PublicacionRegistro>.BadResult(ex.Message, new PublicacionRegistro());
			}
		}

		public async Task<Response<IEnumerable<PublicacionRegistro>>> GetPublicacionesRegistro(PostGetPublicacionesRegistro postModel)
		{
			try
			{
				postModel ??= new PostGetPublicacionesRegistro();
				var request = new spGetPublicacionesRegistro.Request
				{
					vchTipo = string.IsNullOrWhiteSpace(postModel.vchTipo) ? null : postModel.vchTipo.Trim(),
					bExitoso = postModel.bExitoso,
					dtDesde = postModel.dtDesde?.Date,
					dtHasta = postModel.dtHasta?.Date,
					iTop = postModel.iTop > 0 ? postModel.iTop : 200
				};

				var filas = (await _repository.spGetPublicacionesRegistro(request)).ToList();

				// Si el procedimiento falló, el repositorio devuelve una sola fila de error sin identificador.
				var primera = filas.FirstOrDefault();
				if (primera != null && !primera.bResult && primera.iIdPublicacionRegistro == 0)
				{
					return Response<IEnumerable<PublicacionRegistro>>.BadResult(
						string.IsNullOrWhiteSpace(primera.vchMessage) ? "No se pudo consultar el registro de publicaciones." : primera.vchMessage,
						new List<PublicacionRegistro>());
				}

				var datos = filas.Select(f => new PublicacionRegistro
				{
					iIdPublicacionRegistro = f.iIdPublicacionRegistro,
					vchTipo = f.vchTipo,
					iIdReferencia = f.iIdReferencia,
					vchTitulo = f.vchTitulo ?? string.Empty,
					vchCanal = f.vchCanal,
					vchOrigen = f.vchOrigen,
					bExitoso = f.bExitoso,
					vchResultado = f.vchResultado,
					nvchMensaje = f.nvchMensaje,
					vchImagenUrl = f.vchImagenUrl,
					// La base guarda UTC; se marca como tal para que no se convierta dos veces.
					dtPublicacion = DateTime.SpecifyKind(f.dtPublicacion, DateTimeKind.Utc),
					dtFechaLocal = f.dtFechaLocal
				}).ToList();

				return new Response<IEnumerable<PublicacionRegistro>> { IsSuccess = true, Data = datos, Message = "Ok" };
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<PublicacionRegistro>>.BadResult(ex.Message, new List<PublicacionRegistro>());
			}
		}
	}
}
