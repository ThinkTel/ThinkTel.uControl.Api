using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Api
{
    public class ApiClient : IApiClient
    {
		protected Uri uri;
		protected HttpClient client;

		public ApiClient(string uri) : this(new Uri(uri)) { }
		public ApiClient(Uri uri)
		{
			this.uri = uri;

			client = new HttpClient();

			if (!string.IsNullOrEmpty(uri.UserInfo))
			{
				var creds = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(uri.UserInfo));
				var auth = string.Format("{0} {1}", "Basic", creds);
				client.DefaultRequestHeaders.Add("Authorization", auth);
			}
		}

		#region Serialization helpers
		protected async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
		{
			var body = await response.Content.ReadAsByteArrayAsync();
			if (response.IsSuccessStatusCode)
				return Deserialize<T>(body);
			else 
			{
				try 
				{
					var ex = Deserialize<ServerException>(body);
					throw new OperationException(ex);
				} 
				catch(SerializationException ex) 
				{
					throw new ClientException(response.RequestMessage.Method + " " + response.RequestMessage.RequestUri, ex);
				}
			}
		}

		protected T Deserialize<T>(byte[] data)
		{
			if(data == null || data.Length == 0)
				return default(T);
			else
			{
				var serializer = new DataContractSerializer(typeof(T));
				using(var mem = new MemoryStream(data))
					return (T)serializer.ReadObject(mem);
			}
		}

		protected HttpContent Serialize<T>(T obj)
		{
			var serializer = new DataContractSerializer(typeof(T));
			ByteArrayContent content;
			using (var mem = new MemoryStream())
			{
				serializer.WriteObject(mem, obj);
				content = new ByteArrayContent(mem.ToArray());
			}
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
			return content;
		}
		#endregion

		#region HTTP helpers
		protected string MakeUri(string relativeUrl, params object[] args)
		{
			if(args != null && args.Length > 0)
				relativeUrl = string.Format(relativeUrl, args);

			return string.Format("{0}://{1}:{2}{3}/{4}", uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, relativeUrl);
		}

		protected async Task<T> GetAsync<T>(string url, params object[] args)
		{
			var uri = MakeUri(url, args);
			var resp = await client.GetAsync(uri);
			return await DeserializeAsync<T>(resp);
		}

		protected async Task<Tout> PostAsync<Tin, Tout>(Tin request, string url, params object[] args)
		{
			var uri = MakeUri(url, args);
			var body = Serialize(request);
			var resp = await client.PostAsync(uri, body);
			return await DeserializeAsync<Tout>(resp);
		}

		protected async Task<Tout> PutAsync<Tin, Tout>(Tin request, string url, params object[] args)
		{
			var uri = MakeUri(url, args);
			var body = Serialize(request);
			var resp = await client.PutAsync(uri, body);
			return await DeserializeAsync<Tout>(resp);
		}

		protected async Task<T> DeleteAsync<T>(string url, params object[] args)
		{
			var uri = MakeUri(url, args);
			var resp = await client.DeleteAsync(uri);
			return await DeserializeAsync<T>(resp);
		}
		#endregion

		public async Task<long> GetAccountCodeAsync()
		{
			return await GetAsync<long>("Users/Current/Account");
		}

		public async Task<TerseRateCenter[]> ListRateCentersAsync()
        {
            return await GetAsync<TerseRateCenter[]>("RateCenters");
        }

        public async Task<RateCenter> GetRateCenterAsync(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

            return await GetAsync<RateCenter>("RateCenters/{0}", name);
        }

        public async Task<NumberRange[]> ListRateCenterBlocksAsync(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return await GetAsync<NumberRange[]>("RateCenters/{0}/Blocks", name);
        }

        public async Task<NumberItem[]> ListRateCenterNext10Async(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return await GetAsync<NumberItem[]>("RateCenters/{0}/Next10", name);
        }

        public async Task<TerseNumber[]> ListSipTrunkDidsAsync(long pilotNumber)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");

			return await GetAsync<TerseNumber[]>("SipTrunks/{0}/Dids", pilotNumber);
        }

        public async Task<long[]> AddSipTrunkDidsAsync(long pilotNumber, params long[] dids)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (dids == null || dids.Length == 0)
				throw new ArgumentNullException("dids");
			if (dids.Any(x => x <= 0))
				throw new ArgumentException("dids");

			var request = dids.Select(x => new DidRequest { Number = x }).ToArray();
			var response = await PostAsync<DidRequest[], NumberResponse[]>(request, "SipTrunks/{0}/Dids", pilotNumber);
			return ValidateNumberResponses(response);
        }

        public async Task<long[]> AddSipTrunkRateCenterDidsAsync(long pilotNumber, params RateCenterRequest[] request)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (request == null || request.Length == 0)
				throw new ArgumentNullException("request");
			if (request.Any(x => string.IsNullOrEmpty(x.RateCenterName) || x.Quantity <= 0))
				throw new ArgumentException("request");

			var response = await PostAsync<RateCenterRequest[], NumberResponse[]>(request, "SipTrunks/{0}/Dids/NextAvailable", pilotNumber);
			return ValidateNumberResponses(response);
        }

        // RangeRequest[] -> RangeResponse[] POST SipTrunks/{NUMBER}/Dids/Ranges
		public async Task<Did> GetSipTrunkDidAsync(long pilotNumber, long did)
		{
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			return await GetAsync<Did>("SipTrunks/{0}/Dids/{1}", pilotNumber, did);
		}

		public async Task UpdateSipTrunkDidAsync(long pilotNumber, long did, string label, long? translatedNumber = null)
		{
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			var req = new Did { Number = did, Label = label, TranslatedNumber = translatedNumber };
			var response = await PutAsync<Did, NumberResponse>(req, "SipTrunks/{0}/Dids/{1}", pilotNumber, did);
			ValidateNumberResponse(response);
		}

        public async Task CancelSipTrunkDidAsync(long pilotNumber, long did)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			var response = await DeleteAsync<NumberResponse>("SipTrunks/{0}/Dids/{1}", pilotNumber, did);
			ValidateNumberResponse(response);
        }

		#region V911
		public async Task<V911[]> ListV911sAsync()
		{
			return await GetAsync<V911[]>("V911s");
		}
		public async Task AddV911Async(V911 v911)
		{
			var response = await PostAsync<V911, NumberResponse>(v911, "V911s");
			ValidateNumberResponse(response);
		}
		public async Task<V911> GetV911Async(long did)
		{
			return await GetAsync<V911>("V911s/{0}", did);
		}
		public async Task UpdateV911Async(V911 v911)
		{
			var response = await PutAsync<V911, NumberResponse>(v911, "V911s/{0}", v911.Number);
			ValidateNumberResponse(response);
		}
		public async Task CancelV911Async(long did)
		{
			var response = await DeleteAsync<NumberResponse>("V911s/{0}", did);
			ValidateNumberResponse(response);
		}

		public async Task SetV911Async(V911 v911)
		{
			V911 currentV911 = null;
			try
			{
				currentV911 = await GetV911Async(v911.Number);
			}
			catch (OperationException) { }

			if (currentV911 == null)
				await AddV911Async(v911);
			else
				await UpdateV911Async(v911);
		}
		#endregion

		private void ValidateNumberResponse(NumberResponse response)
		{
			if (response.Reply != ResponseCode.Accepted)
				throw new ResponseException(response.ToString());
		}
		private static long[] ValidateNumberResponses(NumberResponse[] response)
		{
			var accepted = response.Where(x => x.Reply == ResponseCode.Accepted).ToArray();
			if (accepted.Length > 0)
				return accepted.Select(x => x.Number).ToArray();
			else
				throw new ResponseException(string.Join(", ", response.Select(x => x.ToString()).ToArray()));
		}
	}
}