using System;

namespace owncloudsharp.Types
{
	public class OCS
	{
		public Meta Meta { get; set; }
		public object Data { get; set; }
	}

	public class Meta {
		public string Status { get; set; }
		public int StatusCode { get; set; }
		public string Message { get; set; }
	}
}

