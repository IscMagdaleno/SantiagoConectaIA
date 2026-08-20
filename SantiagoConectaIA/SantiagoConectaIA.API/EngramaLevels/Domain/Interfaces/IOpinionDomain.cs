using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.OpinionModule;
using SantiagoConectaIA.Share.PostModels.OpinionModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface IOpinionDomain
    {
        Task<Response<IEnumerable<Opinion>>> GetOpiniones(PostGetOpiniones postModel);
        Task<Response<Opinion>> SaveOpinion(PostSaveOpinion postModel, int iIdCiudadano);
    }
}
