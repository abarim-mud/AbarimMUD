using AbarimMUD.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace AbarimMUD.Site.Pages
{
    public class MapModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public int MapId { get; set; }
		public string EncodedImage { get; set; }

		public void OnGet()
		{
			Area map;
			using (var db = Database.CreateDataContext())
			{
				map = (from m in db.Areas.Include(m => m.Rooms).ThenInclude(r => r.Exits) where m.Id == MapId select m).First();
			}

			var mapBuilder = new Utility.Utility.MapBuilder();
			var imageBytes = mapBuilder.Build(map);

			EncodedImage = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
		}
	}
}
