using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Api
{
    public class ApiClient : IApiClient
    {
		RestProxy<ServerException> _proxy;
		public ApiClient(string uri) 
		{
			_proxy = new RestProxy<ServerException>(uri);
		}

        public async Task<TerseRateCenter[]> ListRateCentersAsync()
        {
            return await _proxy.GetAsync<TerseRateCenter[]>("RateCenters");
        }

        public async Task<RateCenter> GetRateCenterAsync(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

            return await _proxy.GetAsync<RateCenter>("RateCenters/{0}", name);
        }

        public async Task<NumberRange[]> ListRateCenterBlocksAsync(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return await _proxy.GetAsync<NumberRange[]>("RateCenters/{0}/Blocks", name);
        }

        public async Task<NumberItem[]> ListRateCenterNext10Async(string name)
        {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return await _proxy.GetAsync<NumberItem[]>("RateCenters/{0}/Next10", name);
        }

        public async Task<TerseNumber[]> ListSipTrunkDidsAsync(long pilotNumber)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");

			return await _proxy.GetAsync<TerseNumber[]>("SipTrunks/{0}/Dids", pilotNumber);
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
			var response = await _proxy.PostAsync<DidRequest[], NumberResponse[]>(request, "SipTrunks/{0}/Dids", pilotNumber);
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

			var response = await _proxy.PostAsync<RateCenterRequest[], NumberResponse[]>(request, "SipTrunks/{0}/Dids/NextAvailable", pilotNumber);
			return ValidateNumberResponses(response);
        }

        // RangeRequest[] -> RangeResponse[] POST SipTrunks/{NUMBER}/Dids/Ranges
		public async Task<Did> GetSipTrunkDidAsync(long pilotNumber, long did)
		{
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			return await _proxy.GetAsync<Did>("SipTrunks/{0}/Dids/{1}", pilotNumber, did);
		}

		public async Task UpdateSipTrunkDidAsync(long pilotNumber, long did, string label, long? translatedNumber = null)
		{
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			var req = new Did { Number = did, Label = label, TranslatedNumber = translatedNumber };
			var response = await _proxy.PutAsync<Did, NumberResponse>(req, "SipTrunks/{0}/Dids/{1}", pilotNumber, did);
			ValidateNumberResponse(response);
		}

        public async Task CancelSipTrunkDidAsync(long pilotNumber, long did)
        {
			if (pilotNumber <= 0)
				throw new ArgumentException("pilotNumber");
			if (did <= 0)
				throw new ArgumentException("did");

			var response = await _proxy.DeleteAsync<NumberResponse>("SipTrunks/{0}/Dids/{1}", pilotNumber, did);
			ValidateNumberResponse(response);
        }

		#region V911
		public async Task<V911[]> ListV911sAsync()
		{
			return await _proxy.GetAsync<V911[]>("V911s");
		}
		public async Task AddV911Async(V911 v911)
		{
			var response = await _proxy.PostAsync<V911, NumberResponse>(v911, "V911s");
			ValidateNumberResponse(response);
		}
		public async Task<V911> GetV911Async(long did)
		{
			return await _proxy.GetAsync<V911>("V911s/{0}", did);
		}
		public async Task UpdateV911Async(V911 v911)
		{
			var response = await _proxy.PutAsync<V911, NumberResponse>(v911, "V911s/{0}", v911.Number);
			ValidateNumberResponse(response);
		}
		public async Task CancelV911Async(long did)
		{
			var response = await _proxy.DeleteAsync<NumberResponse>("V911s/{0}", did);
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

		#region NPA/NXX
		// technically this isn't "uControl", but it's very useful
		public async Task<string> LookupNpaNxxRatecenterAsync(int npa, int nxx)
		{
			return await LocalCallingGuide.LookupNpaNxxRatecenterAsync(npa, nxx);
		}
		#endregion
	}
}