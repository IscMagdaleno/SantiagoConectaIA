using System;
using System.Threading.Tasks;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.BuzonCiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.BuzonCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class BuzonCiudadanoDomain : IBuzonCiudadanoDomain
	{
		private readonly IBuzonCiudadanoRepository _buzonRepository;
		private readonly MapperHelper _mapperHelper;
		private readonly IResponseHelper _responseHelper;

		public BuzonCiudadanoDomain(
			IBuzonCiudadanoRepository buzonRepository,
			MapperHelper mapperHelper,
			IResponseHelper responseHelper)
		{
			_buzonRepository = buzonRepository ?? throw new ArgumentNullException(nameof(buzonRepository));
			_mapperHelper = mapperHelper ?? throw new ArgumentNullException(nameof(mapperHelper));
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
		}

		public async Task<Response<BuzonCiudadano>> RegistrarReporte(PostSaveBuzonCiudadano postModel)
		{
			try
			{
				// Mapear el PostModel a la clase de Request del SP
				var req = _mapperHelper.Get<PostSaveBuzonCiudadano, spSaveBuzonCiudadano.Request>(postModel);

				// Guardar en la base de datos a través del repositorio
				var repoResult = await _buzonRepository.spSaveBuzonCiudadano(req);

				// Validar respuesta y mapear
				var validation = _responseHelper.Validacion<spSaveBuzonCiudadano.Result, BuzonCiudadano>(repoResult);
				if (validation.IsSuccess)
				{
					// Crear el DTO final mapeado a partir del PostModel e inyectar el ID generado
					var reporte = _mapperHelper.Get<PostSaveBuzonCiudadano, BuzonCiudadano>(postModel);
					reporte.iIdReporte = validation.Data.iIdReporte;
					reporte.dtFechaReporte = DateTime.Now;
					validation.Data = reporte;
				}

				return validation;
			}
			catch (Exception ex)
			{
				return Response<BuzonCiudadano>.BadResult(ex.Message, new BuzonCiudadano());
			}
		}
	}
}
