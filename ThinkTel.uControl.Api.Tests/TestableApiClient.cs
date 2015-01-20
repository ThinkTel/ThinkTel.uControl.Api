using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System.Runtime.Serialization;
using System.IO;

namespace ThinkTel.uControl.Api.Tests
{
	public class TestableApiClient : ApiClient
	{
		public class TestingMessageHander : HttpMessageHandler
		{
			public int Called { get; private set; }

			private HttpMethod _expectedMethod;
			private string _expectedUrl;
			private HttpContent _expectedContent;
			private HttpContent _responesContent;
			public TestingMessageHander(HttpMethod expectedMethod, string expectedUrl, HttpContent expectedContent, HttpContent responseContent)
			{
				_expectedMethod = expectedMethod;
				_expectedUrl = expectedUrl;
				_expectedContent = expectedContent;
				_responesContent = responseContent;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				Assert.Equal(_expectedMethod, request.Method);
				Assert.Equal(_expectedUrl, request.RequestUri.ToString());

				if (_expectedContent != null)
				{
					Assert.Equal(_expectedContent.Headers.ContentType.MediaType, request.Content.Headers.ContentType.MediaType);

					var expectedBody = await _expectedContent.ReadAsByteArrayAsync();
					var requestBody = await request.Content.ReadAsByteArrayAsync();
					Assert.Equal(expectedBody, requestBody);
				}

				Called++;

				return new HttpResponseMessage(HttpStatusCode.OK) { Content = _responesContent };
			}
		}

		private TestingMessageHander _testingMessageHandler;
		public TestingMessageHander TestingMessageHandler
		{
			get 
			{
				return _testingMessageHandler;
			}
			set 
			{
				_testingMessageHandler = value;
				client = new HttpClient(value);
			}
		}
		public TestableApiClient(string uri) : base(uri) { }

		public void SetupGet<T>(string url, T resp)
		{
			TestingMessageHandler = new TestingMessageHander(HttpMethod.Get, url, null, GenerateContent(resp));
		}

		public void SetupPost<Tin,Tout>(string url, Tin req, Tout resp)
		{
			TestingMessageHandler = new TestingMessageHander(HttpMethod.Post, url, GenerateContent(req), GenerateContent(resp));
		}

		public void SetupPut<Tin, Tout>(string url, Tin req, Tout resp)
		{
			TestingMessageHandler = new TestingMessageHander(HttpMethod.Put, url, GenerateContent(req), GenerateContent(resp));
		}

		public void SetupDelete<T>(string url, T resp)
		{
			TestingMessageHandler = new TestingMessageHander(HttpMethod.Delete, url, null, GenerateContent(resp));
		}

		private HttpContent GenerateContent<T>(T obj)
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

		public string Username
		{
			get
			{
				var b64auth = client.DefaultRequestHeaders.Authorization.Parameter;
				var auth = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(b64auth));
				var userpass = auth.Split(new char[] { ':' }, 2);
				return userpass[0];
			}
		}

		public string Password
		{
			get
			{
				var b64auth = client.DefaultRequestHeaders.Authorization.Parameter;
				var auth = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(b64auth));
				var userpass = auth.Split(new char[] { ':' }, 2);
				return userpass[1];
			}
		}
	}
}
