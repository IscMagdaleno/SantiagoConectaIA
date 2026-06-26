using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.PageVisitsModule;
using SantiagoConectaIA.Share.PostModels.PageVisitsModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IPageVisitsDomain
	{
		Task<Response<IEnumerable<PageVisitsStat>>> GetPageVisitsStats(PostGetPageVisitsStats postModel);
	}
}
