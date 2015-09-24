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
			var c = new Client ("http://192.168.2.245/owncloud-devel", "admin", "Bno131083");
			/*Console.Write ("Testing DAV:List ... ");
			var list = c.List ("/");
			Console.WriteLine ("Received " + list.Count + " item(s)");
			foreach (var item in list) {
				if (!item.ContentType.Equals ("dav/directory")) {
					Console.Write ("Testing DAV:Downloading: " + item.Path + "/" + item.Name + " ... ");
					var data = c.Download (item.Path + "/" + item.Name);
					Console.WriteLine (data.Length + " Bytes received");
					BinaryReader reader = new BinaryReader (data);
					data.Position = 0;
					byte[] rdata = reader.ReadBytes ((int)data.Length);
					File.WriteAllBytes (item.Name, rdata);
					if (!c.Exists ("/demo"))
						c.CreateDirectory ("/demo");
					Console.Write ("Testing DAV:Uploading " + item.Name + " to /demo/ ... ");
					var status = c.Upload ("/demo/" + item.Name, new MemoryStream (rdata), item.ContentType);
					if (status)
						Console.WriteLine ("DONE");
					else
						Console.WriteLine ("FAILED");
					Console.Write ("Testing DAV:Deleting " + item.Name + " from /demo/ ... ");
					status = c.Delete ("/demo/" + item.Name);
					if (status)
						Console.WriteLine ("DONE");
					else
						Console.WriteLine ("FAILED");
				} else
					continue;
			}
			Console.Write ("Testing OCS:ListOpenRemoteShare ... ");*/
            //c.CreateUser ("test2", "8cs!38s");
            //var ps = c.ShareWithLink("/demo", Convert.ToInt32(OcsPermission.All), "demo", OcsBoolParam.True);
            //var us = c.ShareWithUser("/demo", "test", Convert.ToInt32(OcsPermission.All), OcsBoolParam.False);
            //var gs = c.ShareWithGroup("/demo", "Test123", Convert.ToInt32(OcsPermission.All));
			var result = c.IsShared("/demo") ? "is shared" : "is not shared";
			Console.WriteLine ("/demo " + result + ".");
		}
	}
}
