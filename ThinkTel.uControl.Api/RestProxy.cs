using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Api
{
	public class RestProxy<TException> where TException : class
	{
		public string Username { get; private set; }
		public string Password { get; private set; }
		public Uri ApiUri { get; private set; }
		public static string LogDir
		{
			get;
			set;
		}

		public RestProxy(string uriString)
		{
			this.ApiUri = new Uri(uriString);
			if (!string.IsNullOrEmpty(this.ApiUri.UserInfo))
			{
				string[] array = this.ApiUri.UserInfo.Split(new char[]
				{
					':'
				});
				this.Username = array[0];
				this.Password = array[1];
			}
		}
		public RestProxy(string uriString, string username, string password)
		{
			this.ApiUri = new Uri(uriString);
			this.Username = username;
			this.Password = password;
		}


		public async Task<T> GetAsync<T>(string relativeUrl, params object[] args) where T : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			T result;
			using (WebResponse webResponse = await this.InvokeRequestAsync("GET", relativeUrl))
			{
				result = await this.DeserializeResponseIntoObjectAsync<T>(webResponse, "response");
			}
			return result;
		}

		public async Task PostAsync<T>(T obj, string relativeUrl, params object[] args) where T : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			(await this.InvokeRequestAsync("POST", relativeUrl, async delegate(WebRequest req)
			{
				await this.SerializeObjectIntoStreamAsync<T>("post", req, obj);
			})).Close();
		}

		public async Task<TRes> PostAsync<TReq, TRes>(TReq obj, string relativeUrl, params object[] args) where TRes : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			TRes result;
			using (WebResponse webResponse = await this.InvokeRequestAsync("POST", relativeUrl, async delegate(WebRequest req)
			{
				await this.SerializeObjectIntoStreamAsync<TReq>("post", req, obj);
			}))
			{
				result = await this.DeserializeResponseIntoObjectAsync<TRes>(webResponse, "response");
			}
			return result;
		}

		public async Task<TRes> PostExpectAsync<TReq, TRes>(TReq obj, string relativeUrl, params object[] args)
		{
			if(args != null)
				relativeUrl = string.Format(relativeUrl, args);
			WebResponse webResponse = await this.InvokeRequestAsync("POST", relativeUrl, async delegate(WebRequest req)
			{
				await this.SerializeObjectIntoStreamAsync<TReq>("post", req, obj);
			});
			TRes result;
			using (Stream responseStream = webResponse.GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					if (streamReader.EndOfStream)
					{
						throw new Exception("Missing expected response");
					}
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(TRes));
					byte[] bytes = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
					using (MemoryStream memoryStream = new MemoryStream(bytes))
					{
						result = (TRes)((object)dataContractSerializer.ReadObject(memoryStream));
					}
				}
			}
			return result;
		}

		public async Task PutAsync(string relativeUrl, params object[] args)
		{
			relativeUrl = string.Format(relativeUrl, args);
			(await this.InvokeRequestAsync("PUT", relativeUrl)).Close();
		}

		public async Task PutAsync<T>(T obj, string relativeUrl, params object[] args)
		{
			relativeUrl = string.Format(relativeUrl, args);
			(await this.InvokeRequestAsync("PUT", relativeUrl, async delegate(WebRequest req)
			{
				await this.SerializeObjectIntoStreamAsync<T>("put", req, obj);
			})).Close();
		}

		public async Task<TRes> PutAsync<TReq, TRes>(TReq obj, string relativeUrl, params object[] args) where TRes : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			TRes result;
			using (WebResponse webResponse = await this.InvokeRequestAsync("PUT", relativeUrl, async delegate(WebRequest req)
			{
				await this.SerializeObjectIntoStreamAsync<TReq>("put", req, obj);
			}))
			{
				result = await this.DeserializeResponseIntoObjectAsync<TRes>(webResponse, "response");
			}
			return result;
		}

		public async Task<TRes> PutAsync<TRes>(string relativeUrl, params object[] args) where TRes : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			TRes result;
			using (WebResponse webResponse = await this.InvokeRequestAsync("PUT", relativeUrl))
			{
				result = await this.DeserializeResponseIntoObjectAsync<TRes>(webResponse, "response");
			}
			return result;
		}

		public void Delete(string relativeUrl, params object[] args)
		{
			relativeUrl = string.Format(relativeUrl, args);
			this.InvokeRequest("DELETE", relativeUrl).Close();
		}

		public async Task DeleteAsync(string relativeUrl, params object[] args)
		{
			relativeUrl = string.Format(relativeUrl, args);
			(await this.InvokeRequestAsync("DELETE", relativeUrl)).Close();
		}

		public async Task<TRes> DeleteAsync<TRes>(string relativeUrl, params object[] args) where TRes : class
		{
			relativeUrl = string.Format(relativeUrl, args);
			TRes result;
			using (WebResponse webResponse = await this.InvokeRequestAsync("DELETE", relativeUrl))
			{
				result = await this.DeserializeResponseIntoObjectAsync<TRes>(webResponse, "response");
			}
			return result;
		}

		protected WebResponse InvokeRequest(string method, string relativeUrl, Action<WebRequest> requestSetup = null)
		{
			WebResponse response;
			try
			{
				WebRequest webRequest = WebRequest.Create(new Uri(this.ApiUri, this.ApiUri.AbsolutePath + "/" + relativeUrl));
				webRequest.Credentials = new NetworkCredential(this.Username, this.Password);
				((HttpWebRequest)webRequest).Method = method;
				if (requestSetup != null)
				{
					requestSetup(webRequest);
				}
				else
				{
					if (method != "GET")
					{
						((HttpWebRequest)webRequest).ContentLength = 0L;
					}
				}
				response = webRequest.GetResponse(); // one line difference
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					string respBody = null;
					using (var rdr = new StreamReader(ex.Response.GetResponseStream()))
						respBody = rdr.ReadToEnd();
					try
					{
						using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(respBody)))
						{
							var serializer = new DataContractSerializer(typeof(TException));
							var ex2 = (TException)((object)serializer.ReadObject(mem));
							throw new OperationException(ex2);
						}
					}
					catch (SerializationException innerException)
					{
						throw new ClientException(method + " " + relativeUrl + " " + respBody, innerException);
					}
				}
				throw new ClientException(method + " " + relativeUrl, ex);
			}
			catch (Exception innerException2)
			{
				throw new ClientException(method + " " + relativeUrl, innerException2);
			}
			return response;
		}

		protected async Task<WebResponse> InvokeRequestAsync(string method, string relativeUrl, Action<WebRequest> requestSetup = null)
		{
			WebResponse response;
			try
			{
				WebRequest webRequest = WebRequest.Create(new Uri(this.ApiUri, this.ApiUri.AbsolutePath + "/" + relativeUrl));
				webRequest.Credentials = new NetworkCredential(this.Username, this.Password);
				((HttpWebRequest)webRequest).Method = method;
				if (requestSetup != null)
				{
					requestSetup(webRequest);
				}
				else
				{
					if (method != "GET")
					{
						((HttpWebRequest)webRequest).ContentLength = 0L;
					}
				}
				response = await webRequest.GetResponseAsync();
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					string respBody = null;
					using (var rdr = new StreamReader(ex.Response.GetResponseStream()))
						respBody = rdr.ReadToEnd();
					try
					{
						using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(respBody)))
						{
							var serializer = new DataContractSerializer(typeof(TException));
							var ex2 = (TException)((object)serializer.ReadObject(mem));
							throw new OperationException(ex2);
						}
					}
					catch (SerializationException innerException)
					{
						throw new ClientException(method + " " + relativeUrl + " " + respBody, innerException);
					}
				}
				throw new ClientException(method + " " + relativeUrl, ex);
			}
			catch (Exception innerException2)
			{
				throw new ClientException(method + " " + relativeUrl, innerException2);
			}
			return response;
		}

		protected async Task SerializeObjectIntoStreamAsync<T>(string method, WebRequest req, T obj)
		{
			((HttpWebRequest)req).ContentType = "text/xml";
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
				dataContractSerializer.WriteObject(memoryStream, obj);
				array = memoryStream.ToArray();
			}
			this.LogToFile(array, "{0:yyyyMMdd-HHmmss-ffff}-{1}.xml", new object[]
			{
				DateTime.Now,
				method
			});
			using (Stream requestStream = req.GetRequestStream())
			{
				await requestStream.WriteAsync(array, 0, array.Length);
			}
		}

		protected async Task<T> DeserializeResponseIntoObjectAsync<T>(WebResponse res, string responseType = "response") where T : class
		{
			T result;
			using (Stream responseStream = res.GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					if (streamReader.EndOfStream)
					{
						result = default(T);
					}
					else
					{
						DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
						byte[] bytes = Encoding.UTF8.GetBytes(await streamReader.ReadToEndAsync());
						this.LogToFile(bytes, "{0:yyyyMMdd-HHmmss-ffff}-{1}.xml", new object[]
						{
							DateTime.Now,
							responseType
						});
						using (MemoryStream memoryStream = new MemoryStream(bytes))
						{
							result = (T)((object)dataContractSerializer.ReadObject(memoryStream));
						}
					}
				}
			}
			return result;
		}

		protected void LogToFile(byte[] data, string fileNameFormat, params object[] fileNameArgs)
		{
#if DEBUG
			if (!string.IsNullOrEmpty(LogDir))
			{
				string path = Path.Combine(LogDir, string.Format(fileNameFormat, fileNameArgs));
				using (FileStream fileStream = new FileStream(path, FileMode.Create))
				{
					fileStream.Write(data, 0, data.Length);
				}
			}
#endif
		}
	}
}
