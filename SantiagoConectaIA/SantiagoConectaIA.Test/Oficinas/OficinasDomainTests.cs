using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Core;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository;
using SantiagoConectaIA.Share.PostModels.OficinasModule;

namespace SantiagoConectaIA.Test.Oficinas
{
	public class OficinasDomainTests
	{
		private readonly OficinasRepository _repoMock;
		private readonly MapperHelper _mapperMock;
		private readonly ResponseHelper _respMock;

		private OficinasDomain CreateDomain()
		{
			return new OficinasDomain(_repoMock, _mapperMock, _respMock);
		}

		[Fact]
		public async Task SaveOficina_ReturnsError_WhenLatitudeOutOfRange()
		{
			var domain = CreateDomain();
			var model = new PostSaveOficina { vchNombre = "Test", flLatitud = 100.0, flLongitud = 0.0 };

			var result = await domain.SaveOficina(model);

			Assert.False(result.IsSuccess);
			Assert.Equal("Latitud fuera de rango (-90..90).", result.Message);
		}




		[Fact]
		public async Task SaveOficina_ReturnsSucces()
		{
			var domain = CreateDomain();
			var postModel = new PostSaveOficina
			{
				iIdOficina = 0,
				vchNombre = "Oficina Unitaria",
				vchEmail = "correo@valido.com",
				vchTelefono = "12345678",
				flLatitud = 23.45,
				flLongitud = -102.55
			};

			var result = await domain.SaveOficina(postModel);

			Assert.True(result.IsSuccess);
		}

		// Puedes agregar tests para flujo exitoso (mock spSaveOficina returning success), para Get/ Search, etc.
	}

}
