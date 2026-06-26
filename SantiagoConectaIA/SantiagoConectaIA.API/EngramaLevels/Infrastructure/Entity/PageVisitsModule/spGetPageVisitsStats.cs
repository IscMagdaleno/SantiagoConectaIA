using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PageVisitsModule
{
	public class spGetPageVisitsStats
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetPageVisitsStats";
			public string vchPageName { get; set; } = string.Empty;
			public DateTime? dtStartDate { get; set; }
			public DateTime? dtEndDate { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public string vchPageName { get; set; }
			public int iHour { get; set; }
			public int iTotalVisits { get; set; }
			public int iUniqueVisits { get; set; }
		}
	}
}
