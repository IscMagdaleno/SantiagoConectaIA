using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas
{
	public partial class PageTramites : EngramaPage
	{

		[Inject] public DataTramites Data { get; set; }


	}
}
