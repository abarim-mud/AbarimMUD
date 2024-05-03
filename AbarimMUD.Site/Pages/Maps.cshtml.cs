using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using AbarimMUD.Data;

namespace AbarimMUD.Site.Pages
{
	public class MapsModel : PageModel
	{
		public Area[] Areas { get; set; }

		public void OnGet()
		{
			using (var db = Database.CreateDataContext())
			{
				Areas = db.Areas.ToArray();
			}
		}
	}
}
