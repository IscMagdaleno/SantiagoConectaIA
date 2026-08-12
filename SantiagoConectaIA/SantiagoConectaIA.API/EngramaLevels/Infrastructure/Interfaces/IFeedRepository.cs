using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.FeedModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IFeedRepository
    {
        Task<IEnumerable<spGetFeed.Result>> spGetFeed(spGetFeed.Request daoModel);
        Task<IEnumerable<spSearchFeed.Result>> spSearchFeed(spSearchFeed.Request daoModel);
    }
}
