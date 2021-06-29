using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using WebDav;

using owncloudsharp.Models;

namespace owncloudsharp
{
    public class Client
    {
		#region Members
		/// <summary>
		/// WebDavNet instance.
		/// </summary>
		private WebDavManager dav;
		/// <summary>
		/// ownCloud Base URL.
		/// </summary>
		private string url;
		/// <summary>
		/// ownCloud WebDAV access path.
		/// </summary>
		private const string davpath = "remote.php/webdav";
		/// <summary>
		/// ownCloud OCS API access path.
		/// </summary>
		private const string ocspath = "ocs/v1.php/";
		/// <summary>
		/// OCS Share API path.
		/// </summary>
		private const string ocsServiceShare = "apps/files_sharing/api/v1";
		private const string ocsServiceData = "privatedata";
		/// <summary>
		/// OCS Provisioning API path.
		/// </summary>
		private const string ocsServiceCloud = "cloud";
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Client"/> class.
		/// </summary>
		/// <param name="url">ownCloud instance URL.</param>
		/// <param name="user_id">User identifier.</param>
		/// <param name="password">Password.</param>
		public Client(string url, string user_id, string password)
		{
			// In case URL has a trailing slash remove it
			if ((url != null) && (url.EndsWith("/")))
				url = url.TrimEnd(new[] { '/' });

			// Store ownCloud base URL
			this.url = url;

			// RestSharp initialisation
			//this.rest = new FlurlClient(new Uri(url + "/" + ocspath).ToString())
			//	.WithBasicAuth(user_id, password)
			//	.WithHeader("OCS-APIREQUEST", true)
			//	.AllowAnyHttpStatus();
			//this.rest = new RestClient();
			// Set the base path as the OCS API root
			//this.baseUrl = new Uri (url + "/" + ocspath).ToString();
			// Configure RestSharp for BasicAuth
			//this.rest.Authenticator = new HttpBasicAuthenticator (user_id, password);
			//this.rest.AddDefaultParameter("format", "xml");


			// WebDavNet initialisation
			this.dav = new WebDavManager();
			// Configure WebDavNet for BasicAuth
			this.dav.Credential = new WebDavCredential(user_id, password);
			this.dav.Credential.AuthenticationType = AuthType.Basic;
		}
		#endregion

		#region DAV
		/// <summary>
		/// List the specified remote path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns>List of Resources.</returns>
		public List<ResourceInfo> List(string path)
		{
			List<ResourceInfo> resources = new List<ResourceInfo>();
			var result = this.dav.List(GetDavUri(path));

			foreach (var item in result)
			{
				if (item == result[0]) // Skip the first element, since it is always the root
					continue;

				ResourceInfo res = new ResourceInfo();
				if (item.IsDirectory) // if resource is a directory set special content type
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
				if (!res.ContentType.Equals("dav/directory")) // if resource not a directory, remove the file name from remote path.
					res.Path = res.Path.Replace("/" + res.Name, "");
				resources.Add(res);
			}

			return resources;
		}

		/// <summary>
		/// Gets the resource info for the remote path.
		/// </summary>
		/// <returns>The resource info.</returns>
		/// <param name="path">remote Path.</param>
		public ResourceInfo GetResourceInfo(string path)
		{
			var result = this.dav.List(GetDavUri(path));

			if (result.Count > 0)
			{
				var item = result[0];
				ResourceInfo res = new ResourceInfo();
				if (item.IsDirectory) // if resource is a directory set special content type
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
				if (!res.ContentType.Equals("dav/directory")) // if resource not a directory, remove the file name from remote path.
					res.Path = res.Path.Replace("/" + res.Name, "");
				return res;
			}

			return null;
		}

		/// <summary>
		/// Download the specified file.
		/// </summary>
		/// <param name="path">File remote Path.</param>
		/// <returns>File contents.</returns>
		public Stream Download(string path)
		{
			return dav.DownloadFile(GetDavUri(path));
		}

		/// <summary>
		/// Upload the specified file to the specified path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <param name="data">File contents.</param>
		/// <param name="contentType">File content type.</param>
		/// <returns><c>true</c>, if upload successful, <c>false</c> otherwise.</returns>
		public bool Upload(string path, Stream data, string contentType)
		{
			return dav.UploadFile(GetDavUri(path), data, contentType);
		}

		/// <summary>
		/// Checks if the specified remote path exists.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns><c>true</c>, if remote path exists, <c>false</c> otherwise.</returns>
		public bool Exists(string path)
		{
			return dav.Exists(GetDavUri(path));
		}

		/// <summary>
		/// Creates a new directory at remote path.
		/// </summary>
		/// <returns><c>true</c>, if directory was created, <c>false</c> otherwise.</returns>
		/// <param name="path">remote Path.</param>
		public bool CreateDirectory(string path)
		{
			return dav.CreateDirectory(GetDavUri(path));
		}

		/// <summary>
		/// Delete resource at the specified remote path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns><c>true</c>, if resource was deleted, <c>false</c> otherwise.</returns>
		public bool Delete(string path)
		{
			return dav.Delete(GetDavUri(path));
		}

		/// <summary>
		/// Copy the specified source to destination.
		/// </summary>
		/// <param name="source">Source resoure path.</param>
		/// <param name="destination">Destination resource path.</param>
		/// <returns><c>true</c>, if resource was copied, <c>false</c> otherwise.</returns>
		public bool Copy(string source, string destination)
		{
			return dav.Copy(GetDavUri(source), GetDavUri(destination));
		}

		/// <summary>
		/// Move the specified source and destination.
		/// </summary>
		/// <param name="source">Source resource path.</param>
		/// <param name="destination">Destination resource path.</param>
		/// <returns><c>true</c>, if resource was moved, <c>false</c> otherwise.</returns>
		public bool Move(string source, string destination)
		{
			return dav.Move(GetDavUri(source), GetDavUri(destination));
		}

		/// <summary>
		/// Downloads a remote directory as zip.
		/// </summary>
		/// <returns>The directory as zip.</returns>
		/// <param name="path">path to the remote directory to download.</param>
		public Stream DownloadDirectoryAsZip(string path)
		{
			var client = new WebClient.WebClient();
			client.Credentials = dav.Credential;
			//client.Proxy = dav.Proxy;
			client.Timeout = new TimeSpan(0, 5, 0);

			var uri = GetUri("index.php/apps/files/ajax/download.php?dir=" + WebUtility.UrlEncode(path));
			return client.ExecuteGet(uri);
		}
		#endregion

		#region Url Handling
		/// <summary>
		/// Gets the full URI.
		/// </summary>
		/// <returns>The URI.</returns>
		/// <param name="path">remote Path.</param>
		private Uri GetUri(string path)
		{
			return new Uri(this.url + path);
		}

		/// <summary>
		/// Gets the DAV request URI.
		/// </summary>
		/// <returns>The DAV URI.</returns>
		/// <param name="path">remote Path.</param>
		private Uri GetDavUri(string path)
		{
			return new Uri(this.url + "/" + davpath + path);
		}

		/// <summary>
		/// Gets the remote path for OCS API.
		/// </summary>
		/// <returns>The ocs path.</returns>
		/// <param name="service">Service.</param>
		/// <param name="action">Action.</param>
		private string GetOcsPath(string service, string action)
		{
			var slash = (!service.Equals("")) ? "/" : "";
			return service + slash + action;
		}
		#endregion
	}
}
