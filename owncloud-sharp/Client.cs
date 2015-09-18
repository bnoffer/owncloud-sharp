using System;
using System.Collections.Generic;
using System.IO;

using RestSharp;
using RestSharp.Authenticators;
using WebDav;

using owncloudsharp.Exceptions;
using owncloudsharp.Types;

namespace owncloudsharp
{
	public class Client {
		private RestClient rest;
		private WebDavManager dav;
		private string url;
		private const string davpath = "remote.php/webdav";

		public Client(string url, string user_id, string password) {
			if ((url != null) && (url.EndsWith ("/")))
				url = url.TrimEnd (new []{ '/' });

			this.url = url;
			this.rest = new RestClient ();
			this.rest.BaseUrl = new Uri (url);
			this.rest.Authenticator = new HttpBasicAuthenticator (user_id, password);
			this.dav = new WebDavManager ();
			this.dav.Credential = new WebDavCredential (user_id, password);
			this.dav.Credential.AuthenticationType = AuthType.Basic;
		}

		public List<ResourceInfo> List(string path) {
			List<ResourceInfo> resources = new List<ResourceInfo> ();
			var result = this.dav.List (GetDavUri(path));

			foreach (var item in result) {
				ResourceInfo res = new ResourceInfo ();
				if (item.IsDirectory)
					res.ContentType = "dav/directory";
				else
					res.ContentType = item.ContentType;
				res.Created = item.Created;
				res.ETag = item.Etag;
				res.LastModified = item.Modified;
				res.Name = item.Name;
				res.QuotaAvailable = item.QutoaAvailable;
				res.QuotaUsed = item.QuotaUsed;
				res.Size = item.Size;
				res.Path = item.Uri.AbsolutePath.Replace("/" + davpath, "");
				if (!res.ContentType.Equals ("dav/directory"))
					res.Path = res.Path.Replace ("/" + res.Name, "");
				resources.Add (res);
			}

			return resources;
		}

		public Stream Download(string path) {
			return dav.DownloadFile (GetDavUri (path));
		}

		public bool Upload(string path, Stream data, string contentType) {
			return dav.UploadFile (GetDavUri (path), data, contentType);
		}

		public bool Exists(string path) {
			return dav.Exists (GetDavUri (path));
		}

		public bool CreateDirectory(string path) {
			return dav.CreateDirectory (GetDavUri (path));
		}

		private Uri GetUri(string path) {
			return new Uri (this.url + path);
		}

		private Uri GetDavUri(string path) {
			return new Uri (this.url + "/" + davpath + path);
		}
	}
}

