using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core.EmpresasModule
{
    public class EmpresasDomain : IEmpresasDomain
    {
        private readonly IEmpresasRepository _empresasRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public EmpresasDomain(IEmpresasRepository empresasRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _empresasRepository = empresasRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<Empresa>>> GetEmpresas(PostGetEmpresas postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEmpresas, spGetEmpresas.Request>(postModel);
                var result = await _empresasRepository.spGetEmpresas(request);
                return _responseHelper.Validacion<spGetEmpresas.Result, Empresa>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Empresa>>.BadResult(ex.Message, new List<Empresa>());
            }
        }

        public async Task<Response<Empresa>> SaveEmpresa(PostSaveEmpresa postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveEmpresa, spSaveEmpresa.Request>(postModel);
                var result = await _empresasRepository.spSaveEmpresa(request);
                var validation = _responseHelper.Validacion<spSaveEmpresa.Result, Empresa>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdEmpresa = validation.Data.iIdEmpresa;
                    validation.Data = _mapperHelper.Get<PostSaveEmpresa, Empresa>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<Empresa>.BadResult(ex.Message, new Empresa());
            }
        }
    }
}
