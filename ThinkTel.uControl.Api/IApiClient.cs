using System;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Api
{
    public interface IApiClient
    {
        Task<TerseRateCenter[]> ListRateCentersAsync();
		Task<RateCenter> GetRateCenterAsync(string name);
		Task<NumberRange[]> ListRateCenterBlocksAsync(string name);
		Task<NumberItem[]> ListRateCenterNext10Async(string name);

		Task<TerseNumber[]> ListSipTrunkDidsAsync(long pilotNumber);
		Task<long[]> AddSipTrunkDidsAsync(long pilotNumber, params long[] dids);
		Task<long[]> AddSipTrunkRateCenterDidsAsync(long pilotNumber, params RateCenterRequest[] request);
        // RangeRequest[] -> RangeResponse[] POST SipTrunks/{NUMBER}/Dids/Ranges
		Task<Did> GetSipTrunkDidAsync(long pilotNumber, long did);
		Task UpdateSipTrunkDidAsync(long pilotNumber, long did, string label, long? translatedNumber = null);
		Task CancelSipTrunkDidAsync(long pilotNumber, long did);

		Task<V911[]> ListV911sAsync();
		Task AddV911Async(V911 v911);
		Task<V911> GetV911Async(long did);
		Task UpdateV911Async(V911 v911);
		Task CancelV911Async(long did);
		Task SetV911Async(V911 v911);

		Task<string> LookupNpaNxxRatecenterAsync(int npa, int nxx);
    }
}
