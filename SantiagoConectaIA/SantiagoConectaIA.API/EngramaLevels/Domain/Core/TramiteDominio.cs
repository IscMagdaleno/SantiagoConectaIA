using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository;
using SantiagoConectaIA.Share.DTO_s.TramitesArea;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{


	public class TramiteDominio : ITramiteDominio
	{
		private readonly ITramitesRepository tramitesRepository;
		private readonly MapperHelper mapperHelper;
		private readonly IResponseHelper responseHelper;
		private readonly IOficinasDomain oficinasDomain;

		/// <summary>
		/// Initialize the fields receiving the interfaces on the builder
		/// </summary>
		public TramiteDominio(ITramitesRepository tramitesRepository,
			MapperHelper mapperHelper,
			IResponseHelper responseHelper,
			IOficinasDomain oficinasDomain)
		{
			this.tramitesRepository = tramitesRepository;
			this.mapperHelper = mapperHelper;
			this.responseHelper = responseHelper;
			this.oficinasDomain = oficinasDomain;
		}



		public async Task<Response<IEnumerable<Tramite>>> GetTramites(PostGetTramites PostModel)
		{
			try
			{
				var model = mapperHelper.Get<PostGetTramites, spGetTramites.Request>(PostModel);
				var result = await tramitesRepository.spGetTramites(model);
				var validation = responseHelper.Validacion<spGetTramites.Result, Tramite>(result);
				if (validation.IsSuccess)
				{
					validation.Data = validation.Data;
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Tramite>>.BadResult(ex.Message, new List<Tramite>());
			}
		}


		public async Task<Response<IEnumerable<Tramite>>> SearchTramites(PostSearchTramites PostModel)
		{
			try
			{
				var model = mapperHelper.Get<PostSearchTramites, spSearchTramites.Request>(PostModel);
				var result = await tramitesRepository.spSearchTramites(model);
				var validation = responseHelper.Validacion<spSearchTramites.Result, Tramite>(result);
				if (validation.IsSuccess)
				{
					validation.Data = validation.Data;
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<Tramite>>.BadResult(ex.Message, new List<Tramite>());
			}
		}


		public async Task<Response<Tramite>> SaveTramite(PostSaveTramite PostModel)
		{
			try
			{
				var model = mapperHelper.Get<PostSaveTramite, spSaveTramite.Request>(PostModel);
				var result = await tramitesRepository.spSaveTramite(model);
				var validation = responseHelper.Validacion<spSaveTramite.Result, Tramite>(result);
				if (validation.IsSuccess)
				{
					PostModel.iIdTramite = validation.Data.iIdTramite;
					validation.Data = mapperHelper.Get<PostSaveTramite, Tramite>(PostModel);
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<Tramite>.BadResult(ex.Message, new());
			}
		}



		public async Task<Response<IEnumerable<RequisitosPorTramite>>> GetRequisitosPorTramite(PostGetRequisitosPorTramite PostModel)
		{
			try
			{
				var model = mapperHelper.Get<PostGetRequisitosPorTramite, spGetRequisitosPorTramite.Request>(PostModel);
				var result = await tramitesRepository.spGetRequisitosPorTramite(model);
				var validation = responseHelper.Validacion<spGetRequisitosPorTramite.Result, RequisitosPorTramite>(result);
				if (validation.IsSuccess)
				{
					validation.Data = validation.Data;
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<RequisitosPorTramite>>.BadResult(ex.Message, new List<RequisitosPorTramite>());
			}
		}

		public async Task<Response<RequisitosPorTramite>> SaveRequisito(PostSaveRequisito PostModel)
		{
			try
			{
				var model = mapperHelper.Get<PostSaveRequisito, spSaveRequisito.Request>(PostModel);
				var result = await tramitesRepository.spSaveRequisito(model);
				var validation = responseHelper.Validacion<spSaveRequisito.Result, RequisitosPorTramite>(result);
				if (validation.IsSuccess)
				{
					PostModel.iIdRequisito = validation.Data.iIdRequisito;
					validation.Data = mapperHelper.Get<PostSaveRequisito, RequisitosPorTramite>(PostModel);
				}
				return validation;
			}
			catch (Exception ex)
			{
				return Response<RequisitosPorTramite>.BadResult(ex.Message, new());
			}
		}
		public async Task<Response<IEnumerable<TramitesCardDto>>> GetTramitesCard(PostGetTramites daoModel)
		{
			try
			{
				var request = mapperHelper.Get<PostGetTramites, spGetTramitesCard.Request>(daoModel);
				var result = await tramitesRepository.spGetTramitesCard(request);
				return responseHelper.Validacion<spGetTramitesCard.Result, TramitesCardDto>(result);
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<TramitesCardDto>>.BadResult(ex.Message, new List<TramitesCardDto>());
			}
		}

	}
}
