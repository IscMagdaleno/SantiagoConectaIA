using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.LogsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.LogsModulo;
using SantiagoConectaIA.Share.PostModels.LogsModulo;
using System;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class LogsDomain : ILogsDomain
    {
        private readonly ILogsRepository _logsRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public LogsDomain(ILogsRepository logsRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _logsRepository = logsRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<ApiCallLog>> SaveApiCallLog(PostSaveApiCallLog postModel)
        {
            try
            {
                var model = _mapperHelper.Get<PostSaveApiCallLog, spSaveApiCallLog.Request>(postModel);
                var result = await _logsRepository.spSaveApiCallLog(model);
                var validation = _responseHelper.Validacion<spSaveApiCallLog.Result, ApiCallLog>(result);
                
                if (validation.IsSuccess)
                {
                    postModel.iIdApiCallLog = validation.Data.iIdApiCallLog;
                    // Usually we map back but here ApiCallLog might be the same structure as PostSaveApiCallLog without sensitive/extra data
                    validation.Data = _mapperHelper.Get<PostSaveApiCallLog, ApiCallLog>(postModel);
                }
                
                return validation;
            }
            catch (Exception ex)
            {
                // In a logging failure scenario, we probably don't want to crash the app, but return failure response
                return Response<ApiCallLog>.BadResult(ex.Message, new ApiCallLog());
            }
        }
    }
}
