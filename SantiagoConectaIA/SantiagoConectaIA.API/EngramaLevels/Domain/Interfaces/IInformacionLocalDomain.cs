using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.InformacionLocalModule;
using SantiagoConectaIA.Share.PostModels.InformacionLocalModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface IInformacionLocalDomain
    {
        Task<Response<IEnumerable<InformacionLocal>>> GetInformacionLocal();
        Task<Response<InformacionLocal>> GetInformacionLocalById(int id);
        Task<Response<IEnumerable<InformacionLocal>>> PostGetformacionLocalByText(PostGetInformacionLocal postModel);
        Task<Response<InformacionLocal>> SaveInformacionLocal(PostSaveInformacionLocal postModel);
        Task<Response<IEnumerable<InformacionLocal>>> GetInformacionLocal(PostGetInformacionLocal daoModel);
    }
}
