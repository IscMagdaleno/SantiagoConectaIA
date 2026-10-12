using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
	public class spGetEmpresaRedesSociales
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetEmpresaRedesSociales";
			public int iIdEmpresa { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; } = string.Empty;
			public int iIdRedSocial { get; set; }
			public int iIdEmpresa { get; set; }
			public string vchPlataforma { get; set; } = string.Empty;
			public string vchUrl { get; set; } = string.Empty;
			public bool bActivo { get; set; }
		}
	}
}
