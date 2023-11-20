using System.IO;
using System.Net;
using AbarimMUD.Utils;

namespace AbarimMUD.WebService
{
	public class ServiceClient
	{
		private readonly string _url;

		public ServiceClient(string url)
		{
			_url = url;
		}

		private string DoRequest(string queryString)
		{
			var httpRequest = (HttpWebRequest)WebRequest.Create(_url + "/" + queryString);

			string result;
			using (var response = (HttpWebResponse)httpRequest.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
				}
			}	

			return result;
		}

		public string GetVersion()
		{
			return DoRequest("version");
		}

		public ListAreasResult ListAreas(string account, string password)
		{
			var data = DoRequest(string.Format("listAreas?account={0}&password={1}", account, password));

			return data.DeserializeFromString<ListAreasResult>();
		}

		public GetAreaResult GetArea(string account, string password, int areaId)
		{
			var data = DoRequest(string.Format("getArea?account={0}&password={1}&area={2}", account, password, areaId));

			return data.DeserializeFromString<GetAreaResult>();
			
		}
	}
}
