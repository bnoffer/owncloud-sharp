using System;
using System.IO;

using owncloudsharp;
using owncloudsharp.Types;

namespace ocsharpdemo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var c = new Client ("http://owncloud.example.com", "admin", "demo");
			var list = c.List ("/");
			foreach (var item in list) {
				if (!item.ContentType.Equals ("dav/directory")) {
					var data = c.Download (item.Path + "/" + item.Name);
					BinaryReader reader = new BinaryReader (data);
					data.Position = 0;
					byte[] rdata = reader.ReadBytes ((int)data.Length);
					File.WriteAllBytes (item.Name, rdata);
					if (!c.Exists ("/demo"))
						c.CreateDirectory ("/demo");
					c.Upload ("/demo/" + item.Name, new MemoryStream (rdata), item.ContentType);
				} else
					continue;
			}
		}
	}
}
