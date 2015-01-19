using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Api
{
	public class LocalCallingGuide
	{
		// technically this isn't "uControl", but it's the same source, and it really should be in uControl
		private static Dictionary<int, string> _npaNxxCache = new Dictionary<int, string>();
		// SEE http://en.wikipedia.org/wiki/Toll-free_telephone_number#North_America and http://nanpa.com/pdf/PL_455.pdf
		private static int[] TOLL_FREE_NPAS = new int[] { 800, 888, 877, 866, 855, 844, 833, 822 };
		public static async Task<string> LookupNpaNxxRatecenterAsync(int npa, int nxx)
		{
			if (npa < 199 || npa > 999)
				throw new ArgumentException("npa");
			if (nxx < 199 || nxx > 999)
				throw new ArgumentException("nxx");

			if (TOLL_FREE_NPAS.Contains(npa))
				return "Toll-free";

			var npanxx = npa * 1000 + nxx;
			if (!_npaNxxCache.ContainsKey(npanxx))
			{
				const string URL_TEMPLATE = "http://www.localcallingguide.com/xmlprefix.php?npa={0}&nxx={1}";
				var url = string.Format(URL_TEMPLATE, npa, nxx);
				var resp = await (new HttpClient().GetAsync(url));
				var xml = await resp.Content.ReadAsStringAsync();

				string rc = null, region = null;
				Match m;

				m = Regex.Match(xml, "<rc>(.+)</rc>");
				if (m.Success)
					rc = m.Groups[1].Value;
				if (string.IsNullOrEmpty(rc))
					throw new Exception("Invalid ratecenter for " + npa + " " + nxx);

				m = Regex.Match(xml, "<region>(.+)</region>");
				if (m.Success)
					region = m.Groups[1].Value;
				if (string.IsNullOrEmpty(region))
					throw new Exception("Invalid region for " + npa + " " + nxx);

				_npaNxxCache[npanxx] = string.Format("{0}, {1}", rc, region).Replace('é', 'e');
			}
			return _npaNxxCache[npanxx];
		}
	}
}
