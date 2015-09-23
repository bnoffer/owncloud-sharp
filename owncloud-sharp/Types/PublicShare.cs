using System;

namespace owncloudsharp.Types
{
	public class PublicShare
	{
		public string ShareId { get; set; }
		public string TargetPath { get; set; }
		public string Url { get; set; }
		public string Token { get; set; }

		public PublicShare () {	}
	}
}

