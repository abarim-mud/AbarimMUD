using AbarimMUD.Data;
using AbarimMUD.Site.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AbarimMUD.Site.Pages
{
    public class MapModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public int MapId { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? MaxSteps { get; set; }

		public string EncodedImage { get; set; }

		public void OnGet()
		{
			Area map;
			using (var db = Database.CreateDataContext())
			{
				map = (from m in db.Areas.Include(m => m.Rooms).ThenInclude(r => r.Exits) where m.Id == MapId select m).First();
			}

/*			var rooms = (from r in map.Rooms select new RoomWrapper(r)).ToArray();
			var grid = MapBuilder.Build(rooms, MaxSteps);
			var png = grid.Grid.BuildPng();

			EncodedImage = "data:image/png;base64," + Convert.ToBase64String(png.PngData);*/
		}
	}
}
