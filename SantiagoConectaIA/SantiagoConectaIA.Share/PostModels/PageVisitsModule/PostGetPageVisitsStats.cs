using System;

namespace SantiagoConectaIA.Share.PostModels.PageVisitsModule
{
	public class PostGetPageVisitsStats
	{
		public string vchPageName { get; set; } = string.Empty;
		public DateTime? dtStartDate { get; set; }
		public DateTime? dtEndDate { get; set; }
	}
}
