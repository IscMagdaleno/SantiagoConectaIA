using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.FeedModule;
using SantiagoConectaIA.Share.PostModels.FeedModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IFeedDomain
	{
		Task<Response<IEnumerable<FeedCard>>> GetFeed(PostGetFeed postModel);
		Task<Response<FeedSearchResult>> SearchFeed(PostSearchFeed postModel);
	}
}
