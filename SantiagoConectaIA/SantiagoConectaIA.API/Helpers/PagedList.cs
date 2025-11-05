namespace SantiagoConectaIA.API.Helpers
{
	public class PagedList<T>
	{
		public IEnumerable<T> Items { get; }
		public int Page { get; }
		public int PageSize { get; }
		public int Total { get; }

		public PagedList(IEnumerable<T> items, int page, int pageSize, int total)
		{
			Items = items;
			Page = page;
			PageSize = pageSize;
			Total = total;
		}
	}

}
