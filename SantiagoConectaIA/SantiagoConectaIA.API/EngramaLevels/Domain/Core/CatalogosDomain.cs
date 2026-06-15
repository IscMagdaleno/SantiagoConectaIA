using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CatalogosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.CatalogosModule;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class CatalogosDomain : ICatalogosDomain
    {
        private readonly ICatalogosRepository _catalogosRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public CatalogosDomain(
            ICatalogosRepository catalogosRepository,
            MapperHelper mapperHelper,
            IResponseHelper responseHelper)
        {
            _catalogosRepository = catalogosRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<Catalogo>>> GetCatalogos(PostGetCatalogos postModel)
        {
            try
            {
                var req = _mapperHelper.Get<PostGetCatalogos, spGetCatalogos.Request>(postModel);
                var result = await _catalogosRepository.spGetCatalogos(req);
                return _responseHelper.Validacion<spGetCatalogos.Result, Catalogo>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Catalogo>>.BadResult(ex.Message, new List<Catalogo>());
            }
        }

        public async Task<Response<Parametro>> GetParametroByAlias(PostGetParametro postModel)
        {
            try
            {
                var req = _mapperHelper.Get<PostGetParametro, spGetParametro.Request>(postModel);
                var result = await _catalogosRepository.spGetParametro(req);
                return _responseHelper.Validacion<spGetParametro.Result, Parametro>(result);
            }
            catch (Exception ex)
            {
                return Response<Parametro>.BadResult(ex.Message, new Parametro());
            }
        }
    }
}
