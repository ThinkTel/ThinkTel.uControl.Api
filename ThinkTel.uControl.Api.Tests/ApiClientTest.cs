using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace ThinkTel.uControl.Api.Tests
{
    public class ApiClientTest
    {
		private const string USERNAME = "user";
		private const string PASSWORD = "pass";
		private static readonly string CTOR_URI = string.Format("https://{0}:{1}@localhost/rest.svc", USERNAME, PASSWORD);
		private const string REQUEST_URI = "https://localhost/rest.svc";
		private const string TEST_RC = "Test, ZZ";
		private const long TEST_PILOT = 7005551212;
		private const long TEST_DID = 7005559999;

		private TestableApiClient api;
		public ApiClientTest()
		{
			api = new TestableApiClient(CTOR_URI);
		}

		[Fact]
		public void UsernameAndPasswordFromUri()
		{
			Assert.Equal(USERNAME, api.Username);
			Assert.Equal(PASSWORD, api.Password);
		}

		[Fact]
		public async Task GetAccountCodeAsync()
		{
			var resp = 1234L;
			api.SetupGet(REQUEST_URI + "/Users/Current/Account", resp);
			var actual = await api.GetAccountCodeAsync();
			Assert.Equal(resp, actual);
		}

		[Fact]
		public async Task ListRateCentersAsync()
		{
			var resp = new TerseRateCenter[] {
				new TerseRateCenter { Name = "Edmonton, AB", Country = "CA", Available = 10 },
				new TerseRateCenter { Name = "Toronto, ON", Country = "CA", Available = 20 },
			};

			api.SetupGet(REQUEST_URI + "/RateCenters", resp);

			var actual = await api.ListRateCentersAsync();
			Assert.Equal(resp, actual);
		}

		[Fact]
		public async Task GetRateCenterAsync()
		{
			var resp = new RateCenter { Name = TEST_RC, Country = "CA", Available = 10, Local = true, OnNet = true };

			api.SetupGet(REQUEST_URI + "/RateCenters/" + resp.Name, resp);

			var actual = await api.GetRateCenterAsync(resp.Name);
			Assert.Equal(resp, actual);

			await ThrowsAsync<ArgumentNullException>(async () => await api.GetRateCenterAsync(null));
		}

		[Fact]
		public async Task ListRateCenterBlocksAsync()
		{
			var resp = new NumberRange[] {
				new NumberRange { FirstNumber = 1, LastNumber = 10 },
				new NumberRange { FirstNumber = 100, LastNumber = 123 }
			};

			api.SetupGet(REQUEST_URI + "/RateCenters/" + TEST_RC + "/Blocks", resp);

			var actual = await api.ListRateCenterBlocksAsync(TEST_RC);
			Assert.Equal(resp, actual);

			await ThrowsAsync<ArgumentNullException>(async () => await api.ListRateCenterBlocksAsync(null));
		}

		[Fact]
		public async Task ListRateCenterNext10Async() 
		{
			var resp = new NumberItem[] {
				new NumberItem { Number = 1 }, new NumberItem { Number = 2 }
			};

			api.SetupGet(REQUEST_URI + "/RateCenters/" + TEST_RC + "/Next10", resp);

			var actual = await api.ListRateCenterNext10Async(TEST_RC);
			Assert.Equal(resp, actual);

			await ThrowsAsync<ArgumentNullException>(async () => await api.ListRateCenterNext10Async(null));
		}

		[Fact]
		public async Task ListSipTrunkDidsAsync()
		{
			var resp = new TerseNumber[] {
				new TerseNumber { Number = 1, Label = "Alpha" }, new TerseNumber { Number = 2, Label = "Beta" }
			};

			api.SetupGet(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids", resp);

			var actual = await api.ListSipTrunkDidsAsync(TEST_PILOT);
			Assert.Equal(resp, actual);

			await ThrowsAsync<ArgumentException>(async () => await api.ListSipTrunkDidsAsync(0));
		}

		[Fact]
		public async Task AddSipTrunkDidsAsync()
		{
			var nums = new long[] { 1, 2, 3 };
			var req = nums.Select(x => new DidRequest { Number = x }).ToArray();
			var resp = nums.Select(x => new NumberResponse { Reply = ResponseCode.Accepted, Number = x }).ToArray();

			api.SetupPost(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids", req, resp);

			var actual = await api.AddSipTrunkDidsAsync(TEST_PILOT, nums);
			Assert.Equal(nums, actual);

			await ThrowsAsync<ArgumentException>(async () => await api.AddSipTrunkDidsAsync(0));
			await ThrowsAsync<ArgumentNullException>(async () => await api.AddSipTrunkDidsAsync(TEST_PILOT));
			await ThrowsAsync<ArgumentException>(async () => await api.AddSipTrunkDidsAsync(TEST_PILOT, 0));
		}

		[Fact]
		public async Task AddSipTrunkRateCenterDidsAsync()
		{
			var nums = new long[] { 1, 2, 3 };
			var req = new RateCenterRequest[] {
				new RateCenterRequest { RateCenterName = TEST_RC, Quantity = 3 }
			};
			var resp = nums.Select(x => new NumberResponse { Reply = ResponseCode.Accepted, Number = x }).ToArray();

			api.SetupPost(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids/NextAvailable", req, resp);

			var actual = await api.AddSipTrunkRateCenterDidsAsync(TEST_PILOT, req);
			Assert.Equal(nums, actual);

			await ThrowsAsync<ArgumentException>(async () => await api.AddSipTrunkRateCenterDidsAsync(0));
			await ThrowsAsync<ArgumentNullException>(async () => await api.AddSipTrunkRateCenterDidsAsync(TEST_PILOT));
			await ThrowsAsync<ArgumentException>(async () => 
				await api.AddSipTrunkRateCenterDidsAsync(TEST_PILOT, new RateCenterRequest { Quantity = 0 }));
		}

		[Fact]
		public async Task GetSipTrunkDidAsync()
		{
			var resp = new Did { Number = TEST_DID, Label = "Acme" };

			api.SetupGet(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids/" + resp.Number, resp);

			var actual = await api.GetSipTrunkDidAsync(TEST_PILOT, resp.Number);
			Assert.Equal(resp, actual);

			await ThrowsAsync<ArgumentException>(async () => await api.GetSipTrunkDidAsync(0, resp.Number));
			await ThrowsAsync<ArgumentException>(async () => await api.GetSipTrunkDidAsync(TEST_PILOT, 0));
		}

		[Fact]
		public async Task UpdateSipTrunkDidAsync()
		{
			var req = new Did { Number = TEST_DID, Label = "Acme", TranslatedNumber = 1 };
			var resp = new NumberResponse { Number = TEST_DID, Reply = ResponseCode.Accepted };
			api.SetupPut(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids/" + TEST_DID, req, resp);

			await api.UpdateSipTrunkDidAsync(TEST_PILOT, req.Number, req.Label, req.TranslatedNumber);
			Assert.Equal(1, api.TestingMessageHandler.Called);

			await ThrowsAsync<ArgumentException>(async () => await api.UpdateSipTrunkDidAsync(0, TEST_DID, null, null));
			await ThrowsAsync<ArgumentException>(async () => await api.UpdateSipTrunkDidAsync(TEST_PILOT, 0, null, null));
		}

		[Fact]
		public async Task CancelSipTrunkDidAsync()
		{
			var resp = new NumberResponse { Number = TEST_DID, Reply = ResponseCode.Accepted };
			api.SetupDelete(REQUEST_URI + "/SipTrunks/" + TEST_PILOT + "/Dids/" + TEST_DID, resp);

			await api.CancelSipTrunkDidAsync(TEST_PILOT, TEST_DID);
			Assert.Equal(1, api.TestingMessageHandler.Called);

			await ThrowsAsync<ArgumentException>(async () => await api.CancelSipTrunkDidAsync(0, TEST_DID));
			await ThrowsAsync<ArgumentException>(async () => await api.CancelSipTrunkDidAsync(TEST_PILOT, 0));
		}

		[Fact]
		public async Task ListV911sAsync()
		{
			var resp = new V911[] {
				new V911 
				{ 
					FirstName = "John", LastName = "Doe", Number = TEST_DID, 
					StreetNumber = "123", StreetName = "Any St", City = "Anytown", ProvinceState = "ZZ", PostalZip = "12345" 
				}
			};

			api.SetupGet(REQUEST_URI + "/V911s", resp);

			var actual = await api.ListV911sAsync();
			Assert.Equal(resp, actual);
		}

		[Fact]
		public async Task AddV911Async()
		{
			var req = new V911 
			{ 
				FirstName = "John", LastName = "Doe", Number = TEST_DID, 
				StreetNumber = "123", StreetName = "Any St", City = "Anytown", ProvinceState = "ZZ", PostalZip = "12345" 
			};
			var resp = new NumberResponse { Number = req.Number, Reply = ResponseCode.Accepted };

			api.SetupPost(REQUEST_URI + "/V911s", req, resp);

			await api.AddV911Async(req);
			Assert.Equal(1, api.TestingMessageHandler.Called);
		}

		[Fact]
		public async Task GetV911Async()
		{
			var resp = new V911 
			{ 
				FirstName = "John", LastName = "Doe", Number = TEST_DID, 
				StreetNumber = "123", StreetName = "Any St", City = "Anytown", ProvinceState = "ZZ", PostalZip = "12345" 
			};

			api.SetupGet(REQUEST_URI + "/V911s/" + resp.Number, resp);

			var actual = await api.GetV911Async(resp.Number);
			Assert.Equal(resp, actual);
		}

		[Fact]
		public async Task UpdateV911Async()
		{
			var req = new V911 
			{ 
				FirstName = "John", LastName = "Doe", Number = TEST_DID, 
				StreetNumber = "123", StreetName = "Any St", City = "Anytown", ProvinceState = "ZZ", PostalZip = "12345" 
			};
			var resp = new NumberResponse { Number = req.Number, Reply = ResponseCode.Accepted };

			api.SetupPut(REQUEST_URI + "/V911s/" + req.Number, req, resp);

			await api.UpdateV911Async(req);
			Assert.Equal(1, api.TestingMessageHandler.Called);
		}

		[Fact]
		public async Task CancelV911Async()
		{
			var resp = new NumberResponse { Number = TEST_DID, Reply = ResponseCode.Accepted };

			api.SetupDelete(REQUEST_URI + "/V911s/" + TEST_DID, resp);

			await api.CancelV911Async(TEST_DID);
			Assert.Equal(1, api.TestingMessageHandler.Called);
		}

		private static async Task ThrowsAsync<T>(Func<Task> codeUnderTest) where T : Exception
		{
			try
			{
				await codeUnderTest();
				Assert.Throws<T>(() => { });
			}
			catch (T) { }
		}
	}
}
