using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PageVisitsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.PageVisitsModule;
using SantiagoConectaIA.Share.PostModels.PageVisitsModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class PageVisitsDomain : IPageVisitsDomain
	{
		private readonly IPageVisitsRepository _pageVisitsRepository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public PageVisitsDomain(IPageVisitsRepository pageVisitsRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
		{
			_pageVisitsRepository = pageVisitsRepository;
			_mapperHelper = mapperHelper;
			_responseHelper = responseHelper;
		}

		public async Task<Response<IEnumerable<PageVisitsStat>>> GetPageVisitsStats(PostGetPageVisitsStats postModel)
		{
			try
			{
				var request = _mapperHelper.Get<PostGetPageVisitsStats, spGetPageVisitsStats.Request>(postModel);
				var result = await _pageVisitsRepository.spGetPageVisitsStats(request);
				var response = _responseHelper.Validacion<spGetPageVisitsStats.Result, PageVisitsStat>(result);
				return response;
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<PageVisitsStat>>.BadResult(ex.Message, new List<PageVisitsStat>());
			}
		}
	}
}
