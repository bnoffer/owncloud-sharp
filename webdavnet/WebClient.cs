using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebDav
{
	public class WebClient
	{
		private HttpClientHandler handler;
		public TimeSpan Timeout { get; set; }

		public WebClient ()
		{
			handler = new HttpClientHandler ();
			handler.UseDefaultCredentials = true;
		}

		public HttpResponseMessage Execute(HttpRequestMessage request) {
			return ExecuteAsync (request).Result;
		}

		public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request) {
			HttpClient client = new HttpClient (handler, false);
			client.Timeout = this.Timeout;

			var response = await client.SendAsync (request);
			return response;
		}

		/// <summary>
		/// Gets or sets authentication information used by the web client.
		/// </summary>
		/// <value>authentication information</value>
		public ICredentials Credentials {
			get {
				return handler.Credentials;
			}
			set {
				try {
				handler.Credentials = value;
				handler.UseDefaultCredentials = (handler.Credentials == null) ? true : false;
				} catch (Exception ex) {
					Debug.WriteLine (ex.Message);
				}
			}
		}

		/// <summary>
		/// Gets or sets proxy information used by the web client.
		/// </summary>
		/// <value>proxy information</value>
		public IWebProxy Proxy {
			get {
				return handler.Proxy;
			}
			set {
				handler.Proxy = value;
				handler.UseProxy = (handler.Proxy != null) ? true : false;
			}
		}
	}
}

