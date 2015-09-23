using System;
using System.Net.Http;

namespace WebClient
{
	public class WebMethod : HttpMethod
	{
		public WebMethod (string method) : base(method)
		{
		}

		public static HttpMethod MkCol {
			get {
				return new HttpMethod ("MKCOL");
			}
		}

		public static HttpMethod Copy {
			get {
				return new HttpMethod ("COPY");
			}
		}

		public static HttpMethod Move {
			get {
				return new HttpMethod ("MOVE");
			}
		}

		public static HttpMethod PropFind {
			get {
				return new HttpMethod ("PROPFIND");
			}
		}
	}
}

