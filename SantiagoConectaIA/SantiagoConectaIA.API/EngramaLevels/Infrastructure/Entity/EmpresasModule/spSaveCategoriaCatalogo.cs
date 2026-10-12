using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
	public class spSaveCategoriaCatalogo
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveCategoriaCatalogo";
			public int iIdCategoriaCat { get; set; }
			public int iIdEmpresa { get; set; }
			public string vchNombre { get; set; } = string.Empty;
			public int iOrdenAparicion { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; } = string.Empty;
			public int iIdCategoriaCat { get; set; }
		}
	}
}
