using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
	public class spGetCatalogoEmpresa
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetCatalogoEmpresa";
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; } = string.Empty;
			public int iIdCatalogoEmpresa { get; set; }
			public string vchNombre { get; set; } = string.Empty;
			public string vchIconoUrl { get; set; } = string.Empty;
		}
	}
}
