using EngramaCoreStandar.Extensions;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.InformacionLocalModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.InformacionLocalModule;
using SantiagoConectaIA.Share.PostModels.InformacionLocalModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class InformacionLocalDomain : IInformacionLocalDomain
    {
        private readonly IInformacionLocalRepository _informacionLocalRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public InformacionLocalDomain(
            IInformacionLocalRepository informacionLocalRepository,
            MapperHelper mapperHelper,
            IResponseHelper responseHelper)
        {
            _informacionLocalRepository = informacionLocalRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<InformacionLocal>>> GetInformacionLocal(PostGetInformacionLocal daoModel)
        {
            try
            {
                var model = _mapperHelper.Get<PostGetInformacionLocal, spGetInformacionLocal.Request>(daoModel);
                var result = await _informacionLocalRepository.spGetInformacionLocal(model);
                return _responseHelper.Validacion<spGetInformacionLocal.Result, InformacionLocal>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<InformacionLocal>>.BadResult(ex.Message, new List<InformacionLocal>());
            }
        }
        
        public async Task<Response<IEnumerable<InformacionLocal>>> GetInformacionLocal()
        {
            try
            {
                var request = new spGetInformacionLocal.Request
                {
                    nvchCategoria = null,
                    nvchTexto = null,
                    bActivo = true
                };

                var result = await _informacionLocalRepository.spGetInformacionLocal(request);

                if (result == null || !result.Any())
                    return new Response<IEnumerable<InformacionLocal>> { IsSuccess = false, Message = "No se encontraron registros.", Data = Enumerable.Empty<InformacionLocal>() };

                var first = result.First();
                if (!first.bResult)
                    return Response<IEnumerable<InformacionLocal>>.BadResult(first.vchMessage, Enumerable.Empty<InformacionLocal>());

                var data = result.Select(MapToDto).ToList();
                return new Response<IEnumerable<InformacionLocal>> { IsSuccess = true, Data = data, Message = "Ok" };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<InformacionLocal>>.BadResult(ex.Message, Enumerable.Empty<InformacionLocal>());
            }
        }

        public async Task<Response<InformacionLocal>> GetInformacionLocalById(int id)
        {
            try
            {
                var request = new spGetInformacionLocalById.Request { iIdInformacionLocal = id };
                var result = await _informacionLocalRepository.spGetInformacionLocalById(request);

                if (result == null || !result.bResult)
                    return Response<InformacionLocal>.BadResult(result?.vchMessage ?? "Registro no encontrado.", new InformacionLocal());

                return new Response<InformacionLocal> { IsSuccess = true, Data = MapToDto(result), Message = "Ok" };
            }
            catch (Exception ex)
            {
                return Response<InformacionLocal>.BadResult(ex.Message, new InformacionLocal());
            }
        }

        public async Task<Response<IEnumerable<InformacionLocal>>> PostGetformacionLocalByText(PostGetInformacionLocal postModel)
        {
			try
			{
				var request = new spGetInformacionLocal.Request
				{
					nvchCategoria = postModel.nvchCategoria,
					nvchTexto = postModel.nvchTexto,
					bActivo = postModel.bActivo
				};

				var result = await _informacionLocalRepository.spGetInformacionLocal(request);

				if (result == null || !result.Any())
					return new Response<IEnumerable<InformacionLocal>> { IsSuccess = false, Message = "No se encontraron registros.", Data = Enumerable.Empty<InformacionLocal>() };

				var first = result.First();
				if (!first.bResult)
					return Response<IEnumerable<InformacionLocal>>.BadResult(first.vchMessage, Enumerable.Empty<InformacionLocal>());

				var data = result.Select(MapToDto).ToList();
				return new Response<IEnumerable<InformacionLocal>> { IsSuccess = true, Data = data, Message = "Ok" };
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<InformacionLocal>>.BadResult(ex.Message, Enumerable.Empty<InformacionLocal>());
			}
		}

        public async Task<Response<InformacionLocal>> SaveInformacionLocal(PostSaveInformacionLocal postModel)
        {
            try
            {
                var request = new spSaveInformacionLocal.Request
                {
                    iIdInformacionLocal = postModel.iIdInformacionLocal,
                    nvchCategoria = postModel.nvchCategoria,
                    nvchTitulo = postModel.nvchTitulo,
                    nvchPalabrasClave = postModel.nvchPalabrasClave,
                    nvchDescripcionCorta = postModel.nvchDescripcionCorta,
                    nvchContenidoDetallado = postModel.nvchContenidoDetallado,
                    nvchUbicacion_LatLong = postModel.nvchUbicacion_LatLong,
                    bActivo = postModel.bActivo
                };

                var result = await _informacionLocalRepository.spSaveInformacionLocal(request);

                if (result == null || !result.bResult)
                    return Response<InformacionLocal>.BadResult(result?.vchMessage ?? "Error al guardar.", new InformacionLocal());

                postModel.iIdInformacionLocal = result.iIdInformacionLocal;
                var data = new InformacionLocal
                {
                    iIdInformacionLocal = result.iIdInformacionLocal,
                    nvchCategoria = postModel.nvchCategoria,
                    nvchTitulo = postModel.nvchTitulo,
                    nvchPalabrasClave = postModel.nvchPalabrasClave,
                    nvchDescripcionCorta = postModel.nvchDescripcionCorta,
                    nvchContenidoDetallado = postModel.nvchContenidoDetallado,
                    nvchUbicacion_LatLong = postModel.nvchUbicacion_LatLong,
                    bActivo = postModel.bActivo
                };

                return new Response<InformacionLocal> { IsSuccess = true, Data = data, Message = result.vchMessage };
            }
            catch (Exception ex)
            {
                return Response<InformacionLocal>.BadResult(ex.Message, new InformacionLocal());
            }
        }

        private static InformacionLocal MapToDto(spGetInformacionLocal.Result r)
        {
            return new InformacionLocal
            {
                iIdInformacionLocal = r.iIdInformacionLocal,
                nvchCategoria = r.nvchCategoria,
                nvchTitulo = r.nvchTitulo,
                nvchPalabrasClave = r.nvchPalabrasClave,
                nvchDescripcionCorta = r.nvchDescripcionCorta,
                nvchContenidoDetallado = r.nvchContenidoDetallado,
                nvchUbicacion_LatLong = r.nvchUbicacion_LatLong,
                dtFechaCreacion = r.dtFechaCreacion,
                bActivo = r.bActivo
            };
        }

        private static InformacionLocal MapToDto(spGetInformacionLocalById.Result r)
        {
            return new InformacionLocal
            {
                iIdInformacionLocal = r.iIdInformacionLocal,
                nvchCategoria = r.nvchCategoria,
                nvchTitulo = r.nvchTitulo,
                nvchPalabrasClave = r.nvchPalabrasClave,
                nvchDescripcionCorta = r.nvchDescripcionCorta,
                nvchContenidoDetallado = r.nvchContenidoDetallado,
                nvchUbicacion_LatLong = r.nvchUbicacion_LatLong,
                dtFechaCreacion = r.dtFechaCreacion,
                bActivo = r.bActivo
            };
        }
    }
}
