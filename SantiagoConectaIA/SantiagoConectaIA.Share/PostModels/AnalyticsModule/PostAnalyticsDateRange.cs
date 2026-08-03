using System;

namespace SantiagoConectaIA.Share.PostModels.AnalyticsModule
{
	public class PostAnalyticsDateRange
	{
		public DateTime? dtStartDate { get; set; }
		public DateTime? dtEndDate { get; set; }
	}

	public class PostAnalyticsTopRows
	{
		public int iTopRows { get; set; } = 100;
	}
}
