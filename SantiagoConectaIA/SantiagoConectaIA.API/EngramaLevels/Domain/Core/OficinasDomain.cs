using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.Helpers;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.PostModels.OficinasModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class OficinasDomain : IOficinasDomain
	{
		private readonly IOficinasRepository _oficinasRepository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public OficinasDomain(
			IOficinasRepository oficinasRepository,
			MapperHelper mapperHelper,
			IResponseHelper responseHelper)
		{
			_oficinasRepository = oficinasRepository ?? throw new ArgumentNullException(nameof(oficinasRepository));
			_mapperHelper = mapperHelper ?? throw new ArgumentNullException(nameof(mapperHelper));
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
		}

		public async Task<Response<IEnumerable<Oficina>>> GetOficinas(PostGetOficinas postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetOficinas, spGetOficinas.Request>(postModel);
				var repoResult = await _oficinasRepository.spGetOficinas(req);
				var validation = _responseHelper.Validacion<spGetOficinas.Result, Oficina>(repoResult);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Oficina>>.BadResult(ex.Message, Enumerable.Empty<Oficina>());
			}
		}

		public async Task<Response<PagedList<Oficina>>> SearchOficinas(PostSearchOficinas postModel)
		{
			try
			{
				// Normalizar texto
				postModel.vchTexto = string.IsNullOrWhiteSpace(postModel.vchTexto) ? null : postModel.vchTexto.Trim();

				var req = _mapperHelper.Get<PostSearchOficinas, spSearchOficinas.Request>(postModel);
				var repoData = await _oficinasRepository.spSearchOficinas(req);

				// Validación y mapeo
				var validation = _responseHelper.Validacion<spSearchOficinas.Result, Oficina>(repoData);
				if (!validation.IsSuccess)
					return Response<PagedList<Oficina>>.BadResult(validation.Message, new PagedList<Oficina>(Enumerable.Empty<Oficina>(), postModel.iPage, postModel.iPageSize, 0));

				var list = validation.Data ?? Enumerable.Empty<Oficina>();
				// Nota: spSearchOficinas no devuelve count total en nuestro diseño; asumimos page results only
				var paged = new PagedList<Oficina>(list, postModel.iPage, postModel.iPageSize, list.Count());
				return new Response<PagedList<Oficina>> { IsSuccess = true, Data = paged };
			}
			catch (Exception ex)
			{
				return Response<PagedList<Oficina>>.BadResult(ex.Message, new PagedList<Oficina>(Enumerable.Empty<Oficina>(), 1, 1, 0));
			}
		}

		public async Task<Response<Oficina>> SaveOficina(PostSaveOficina postModel)
		{
			try
			{
				// Mapear el modelo a la entidad del SP
				var req = _mapperHelper.Get<PostSaveOficina, spSaveOficina.Request>(postModel);

				// Llamar directamente al SP (todas las validaciones estarán en SQL)
				var repoResult = await _oficinasRepository.spSaveOficina(req);

				// Validar resultado y mapear
				var validation = _responseHelper.Validacion<spSaveOficina.Result, Oficina>(repoResult);
				if (validation.IsSuccess)
				{
					postModel.iIdOficina = validation.Data.iIdOficina;
					validation.Data = _mapperHelper.Get<PostSaveOficina, Oficina>(postModel);
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<Oficina>.BadResult(ex.Message, new Oficina());
			}
		}


		public async Task<Response<Oficina>> LinkOficinaTramite(PostLinkOficinaTramite postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostLinkOficinaTramite, spLinkOficinaTramite.Request>(postModel);
				var repo = await _oficinasRepository.spLinkOficinaTramite(req);
				var validation = _responseHelper.Validacion<spLinkOficinaTramite.Result, Oficina>(repo);
				if (validation.IsSuccess)
				{
					postModel.iIdOficina = validation.Data.iIdOficina;
					validation.Data = _mapperHelper.Get<PostLinkOficinaTramite, Oficina>(postModel);
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<Oficina>.BadResult(ex.Message, new Oficina());
			}
		}

		public async Task<Response<IEnumerable<Oficina>>> GetOficinasPorTramite(PostGetOficinasPorTramite postModel)
		{
			try
			{
				var req = _mapperHelper.Get<PostGetOficinasPorTramite, spGetOficinasPorTramite.Request>(postModel);
				var repo = await _oficinasRepository.spGetOficinasPorTramite(req);
				var validation = _responseHelper.Validacion<spGetOficinasPorTramite.Result, Oficina>(repo);
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Oficina>>.BadResult(ex.Message, Enumerable.Empty<Oficina>());
			}
		}
	}

}
