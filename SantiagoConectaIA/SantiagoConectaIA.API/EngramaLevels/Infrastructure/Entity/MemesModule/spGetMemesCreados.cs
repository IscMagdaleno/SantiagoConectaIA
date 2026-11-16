using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule
{
	public class spGetMemesCreados
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetMemesCreados";
			public int iTop { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdMeme { get; set; }
			public string vchPlantillaId { get; set; }
			public string nvchPlantillaNombre { get; set; }
			public string vchPlantillaUrl { get; set; }
			public string nvchIdea { get; set; }
			public string vchResultadoUrl { get; set; }
			public DateTime dtCreacion { get; set; }
		}
	}
}
