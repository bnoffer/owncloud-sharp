using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebClient
{
	public class WebClient
	{
		/// <summary>
		/// HTTP Handler instance.
		/// </summary>
		private HttpClientHandler handler;
		/// <summary>
		/// Request Timeout.
		/// </summary>
		/// <value>The timeout.</value>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebClient.WebClient"/> class.
		/// </summary>
		public WebClient ()
		{
			handler = new HttpClientHandler ();
			//handler.PreAuthenticate = true;
			handler.UseDefaultCredentials = true;
		}

		/// <summary>
		/// Execute specified HTTP request.
		/// </summary>
		/// <returns>HTTP Response.</returns>
		/// <param name="request">HTTP Request.</param>
		public HttpResponseMessage Execute(HttpRequestMessage request) {
			return ExecuteAsync (request).Result;
		}

		/// <summary>
		/// Executes a HTTP Get request.
		/// </summary>
		/// <returns>Data received.</returns>
		/// <param name="uri">Request URI.</param>
		public Stream ExecuteGet(Uri uri) {
			HttpRequestMessage webRequest = new HttpRequestMessage (WebMethod.Get, uri);
			webRequest.Headers.Add("Pragma", "no-cache");
			HttpResponseMessage webResponse;

			MemoryStream remoteStream = new MemoryStream();

			try
			{
				webResponse = Execute(webRequest);
			}
			catch (Exception e)
			{
				// TODO: Errorhandling
				Debug.WriteLine(e.Message);

				return null;
			}

			if (webResponse == null)
			{
				// TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse " + Environment.NewLine + uri + Environment.NewLine);

				return null;
			}

			try
			{
				// Get the stream object of the response
				webResponse.Content.ReadAsStreamAsync().Result.CopyTo(remoteStream);
			}
			catch (Exception e)
			{
				// TODO: Errorhandling
				Debug.WriteLine(e.Message);

				return null;
			}

			return remoteStream;
		}

		/// <summary>
		/// Executes a HTTP reqquest async.
		/// </summary>
		/// <returns>HTTP Response.</returns>
		/// <param name="request">HTTP Request.</param>
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

