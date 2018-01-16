using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;
using Flurl.Util;

using WebDav;
using WebClient;

using owncloudsharp.Exceptions;
using owncloudsharp.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace owncloudsharp
{
	/// <summary>
	/// ownCloud OCS and DAV access client
	/// </summary>
	public class Client {
		#region Members
		/// <summary>
		/// RestSharp instance.
		/// </summary>
        private FlurlClient rest;
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
		public Client(string url, string user_id, string password) {
			// In case URL has a trailing slash remove it
			if ((url != null) && (url.EndsWith ("/")))
				url = url.TrimEnd (new []{ '/' });

			// Store ownCloud base URL
			this.url = url;

            // RestSharp initialisation
            this.rest = new FlurlClient(new Uri(url + "/" + ocspath).ToString())
                .WithBasicAuth(user_id, password)
                .WithHeader("OCS-APIREQUEST", true)
                .AllowAnyHttpStatus();
            //this.rest = new RestClient();
            // Set the base path as the OCS API root
            //this.baseUrl = new Uri (url + "/" + ocspath).ToString();
            // Configure RestSharp for BasicAuth
            //this.rest.Authenticator = new HttpBasicAuthenticator (user_id, password);
            //this.rest.AddDefaultParameter("format", "xml");


			// WebDavNet initialisation
			this.dav = new WebDavManager ();
			// Configure WebDavNet for BasicAuth
			this.dav.Credential = new WebDavCredential (user_id, password);
			this.dav.Credential.AuthenticationType = AuthType.Basic;
		}
		#endregion

		#region DAV
		/// <summary>
		/// List the specified remote path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns>List of Resources.</returns>
		public List<ResourceInfo> List(string path) {
			List<ResourceInfo> resources = new List<ResourceInfo> ();
			var result = this.dav.List (GetDavUri(path));

			foreach (var item in result) {
				if (item == result[0]) // Skip the first element, since it is always the root
					continue;
				
				ResourceInfo res = new ResourceInfo ();
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
				if (!res.ContentType.Equals ("dav/directory")) // if resource not a directory, remove the file name from remote path.
					res.Path = res.Path.Replace ("/" + res.Name, "");
				resources.Add (res);
			}

			return resources;
		}

		/// <summary>
		/// Gets the resource info for the remote path.
		/// </summary>
		/// <returns>The resource info.</returns>
		/// <param name="path">remote Path.</param>
		public ResourceInfo GetResourceInfo(string path) {
			var result = this.dav.List (GetDavUri(path));

			if (result.Count > 0) {
				var item = result [0];
				ResourceInfo res = new ResourceInfo ();
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
				if (!res.ContentType.Equals ("dav/directory")) // if resource not a directory, remove the file name from remote path.
					res.Path = res.Path.Replace ("/" + res.Name, "");
				return res;
			}

			return null;
		}

		/// <summary>
		/// Download the specified file.
		/// </summary>
		/// <param name="path">File remote Path.</param>
		/// <returns>File contents.</returns>
		public Stream Download(string path) {
			return dav.DownloadFile (GetDavUri (path));
		}

		/// <summary>
		/// Upload the specified file to the specified path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <param name="data">File contents.</param>
		/// <param name="contentType">File content type.</param>
		/// <returns><c>true</c>, if upload successful, <c>false</c> otherwise.</returns>
		public bool Upload(string path, Stream data, string contentType) {
			return dav.UploadFile (GetDavUri (path), data, contentType);
		}

		/// <summary>
		/// Checks if the specified remote path exists.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns><c>true</c>, if remote path exists, <c>false</c> otherwise.</returns>
		public bool Exists(string path) {
			return dav.Exists (GetDavUri (path));
		}

		/// <summary>
		/// Creates a new directory at remote path.
		/// </summary>
		/// <returns><c>true</c>, if directory was created, <c>false</c> otherwise.</returns>
		/// <param name="path">remote Path.</param>
		public bool CreateDirectory(string path) {
			return dav.CreateDirectory (GetDavUri (path));
		}

		/// <summary>
		/// Delete resource at the specified remote path.
		/// </summary>
		/// <param name="path">remote Path.</param>
		/// <returns><c>true</c>, if resource was deleted, <c>false</c> otherwise.</returns>
		public bool Delete(string path) {
			return dav.Delete (GetDavUri (path));
		}

		/// <summary>
		/// Copy the specified source to destination.
		/// </summary>
		/// <param name="source">Source resoure path.</param>
		/// <param name="destination">Destination resource path.</param>
		/// <returns><c>true</c>, if resource was copied, <c>false</c> otherwise.</returns>
		public bool Copy(string source, string destination) {
			return dav.Copy(GetDavUri(source), GetDavUri(destination));
		}

		/// <summary>
		/// Move the specified source and destination.
		/// </summary>
		/// <param name="source">Source resource path.</param>
		/// <param name="destination">Destination resource path.</param>
		/// <returns><c>true</c>, if resource was moved, <c>false</c> otherwise.</returns>
		public bool Move(string source, string destination) {
			return dav.Move(GetDavUri(source), GetDavUri(destination));
		}

		/// <summary>
		/// Downloads a remote directory as zip.
		/// </summary>
		/// <returns>The directory as zip.</returns>
		/// <param name="path">path to the remote directory to download.</param>
		public Stream DownloadDirectoryAsZip(string path) {
			var client = new WebClient.WebClient ();
			client.Credentials = dav.Credential;
			//client.Proxy = dav.Proxy;
			client.Timeout = new TimeSpan(0, 5, 0);

			var uri = GetUri ("index.php/apps/files/ajax/download.php?dir=" + WebUtility.UrlEncode(path));
			return client.ExecuteGet (uri);
		}
		#endregion

		#region OCS
		#region Remote Shares
		/// <summary>
		/// List all remote shares.
		/// </summary>
		/// <returns>List of remote shares.</returns>
		public object ListOpenRemoteShare() {
            var urlSegments = GetOcsPath(ocsServiceShare, "remote_shares");
            var request = rest.Request(urlSegments);

            var response = request.GetJsonAsync<OCS>();
            /*var request = new RestRequest(, Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");
			var response = rest.Execute (request).Result;*/

            response.Wait();

			CheckOcsStatus (response);

            var content = response.Result; 
			// TODO: Parse response
            return content;
		}

		/// <summary>
		/// Accepts a remote share
		/// </summary>
		/// <returns><c>true</c>, if remote share was accepted, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		public bool AcceptRemoteShare(int shareId) {
            var urlSegments = GetOcsPath(ocsServiceShare, "remote_shares");
            var request = rest.Request(urlSegments);

            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            /*var request = new RestRequest(GetOcsPath(ocsServiceShare, "remote_shares") + "/{id}", Method.POST);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");
            
			var response = rest.Execute<OCS>(request).Result;*/
            
            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Declines a remote share.
		/// </summary>
		/// <returns><c>true</c>, if remote share was declined, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		public bool DeclineRemoteShare(int shareId) {
            var urlSegments = GetOcsPath(ocsServiceShare, "remote_shares");
            var request = rest.Request(urlSegments + String.Format("/{0}", shareId + ""));

            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            /*var request = new RestRequest(GetOcsPath(ocsServiceShare, "remote_shares") + "/{id}", Method.DELETE);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request).Result;*/
            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion

		#region SHares
		/// <summary>
		/// Unshares a file or directory.
		/// </summary>
		/// <returns><c>true</c>, if share was deleted, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		public bool DeleteShare(int shareId) {
            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments + String.Format("/{0}", shareId + ""));

            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            /*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares") + "/{id}", Method.DELETE);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request).Result;*/
            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Updates a given share. NOTE: Only one of the update parameters can be specified at once.
		/// </summary>
		/// <returns><c>true</c>, if share was updated, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		/// <param name="perms">(optional) update permissions.</param>
		/// <param name="password">(optional) updated password for public link Share.</param>
		/// <param name="public_upload">(optional) If set to <c>true</c> enables public upload for public shares.</param>
		public bool UpdateShare(int shareId, int perms = -1, string password = null, OcsBoolParam public_upload = OcsBoolParam.None) {
			if ((perms == Convert.ToInt32(OcsPermission.None)) && (password == null) && (public_upload == OcsBoolParam.None))
				return false;
			
            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments + String.Format("/{0}", shareId + ""));

            /*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares") + "/{id}", Method.PUT);
			
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            if (perms != Convert.ToInt32(OcsPermission.None))
                parameters.Add(new { permissions = perms });
            if (password != null)
                parameters.Add(new { password = password });
            if (public_upload == OcsBoolParam.True)
                parameters.Add(new { publicUpload = true });
            else if (public_upload == OcsBoolParam.False)
                parameters.Add(new { publicUpload = false });

            request.SetQueryParams(parameters.ToArray());

			/*if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddQueryParameter ("permissions", Convert.ToInt32(perms) + "");
			if (password != null)
				request.AddQueryParameter ("password", password);
			if (public_upload == OcsBoolParam.True)
				request.AddQueryParameter ("publicUpload", "true");
			else if (public_upload == OcsBoolParam.False)
				request.AddQueryParameter ("publicUpload", "false");
            
			var response = rest.Execute<OCS>(request).Result;*/
            var response = request.PutAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Shares a remote file with link.
		/// </summary>
		/// <returns>instance of PublicShare with the share info.</returns>
		/// <param name="path">path to the remote file to share.</param>
		/// <param name="perms">(optional) permission of the shared object.</param>
		/// <param name="password">(optional) sets a password.</param>
		/// <param name="public_upload">(optional) allows users to upload files or folders.</param>
		public PublicShare ShareWithLink(string path, int perms = -1, string password = null, OcsBoolParam public_upload = OcsBoolParam.None) {
            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { shareType = Convert.ToInt32(OcsShareType.Link) });
            parameters.Add(new { path = path });

            if (perms != Convert.ToInt32(OcsPermission.None))
                parameters.Add(new { permissions = perms });
            if (password != null)
                parameters.Add(new { password = password });
            if (public_upload == OcsBoolParam.True)
                parameters.Add(new { publicUpload = true });
            else if (public_upload == OcsBoolParam.False)
                parameters.Add(new { publicUpload = false });
			/*request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Link));
			request.AddParameter ("path", path);

			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", Convert.ToInt32(perms) + "");
			if (password != null)
				request.AddParameter ("password", password);
			if (public_upload == OcsBoolParam.True)
				request.AddParameter ("publicUpload", "true");
			else if (public_upload == OcsBoolParam.False)
				request.AddParameter ("publicUpload", "false");

            var response = rest.Execute (request).Result;*/
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

			CheckOcsStatus (response);

			/*PublicShare share = new PublicShare ();
			share.ShareId = Convert.ToInt32(GetFromData(response.Content, "id"));
			share.Url = GetFromData(response.Content, "url");
			share.Token = GetFromData(response.Content, "token");
			share.TargetPath = path;
            share.Perms = (perms > -1) ? perms : Convert.ToInt32(OcsPermission.Read);*/

            return (PublicShare)response.Result.Data;
		}

		/// <summary>
		/// Shares a remote file with specified user.
		/// </summary>
		/// <returns>instance of UserShare with the share info.</returns>
		/// <param name="path">path to the remote file to share.</param>
		/// <param name="username">name of the user whom we want to share a file/folder.</param>
		/// <param name="perms">permissions of the shared object.</param>
		/// <param name="remoteUser">Remote user.</param>
		public UserShare ShareWithUser(string path, string username, int perms = -1, OcsBoolParam remoteUser = OcsBoolParam.None) {
			if ((perms == -1) || (perms > Convert.ToInt32 (OcsPermission.All)) || (username == null) || (username.Equals ("")))
				return null;

            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments);
			/*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            if (remoteUser == OcsBoolParam.True)
                parameters.Add(new { shareType = Convert.ToInt32(OcsShareType.Remote) });
            else
                parameters.Add(new { shareType = Convert.ToInt32(OcsShareType.User) });
            parameters.Add(new { path = path });
            if (perms != Convert.ToInt32(OcsPermission.None))
                parameters.Add(new { permissions = perms });
            else
                parameters.Add(new { permissions = Convert.ToInt32(OcsPermission.Read) });
            parameters.Add(new { shareWith = username });

			/*if (remoteUser == OcsBoolParam.True)
				request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Remote));
			else
				request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.User));
			request.AddParameter ("path", path);
			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", perms + "");
			else
				request.AddParameter("permissions", Convert.ToInt32(OcsPermission.Read) + "");
			request.AddParameter ("shareWith", username);*/

			//var response = rest.Execute (request).Result;
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            /*var share = new UserShare();
			share.ShareId = Convert.ToInt32(GetFromData(response.Content, "id"));
            share.TargetPath = path;
            share.Perms = perms;
			share.SharedWith = username;*/

            return (UserShare)response.Result.Data;
		}

		/// <summary>
		/// Shares a remote file with specified group.
		/// </summary>
		/// <returns>instance of GroupShare with the share info.</returns>
		/// <param name="path">path to the remote file to share.</param>
		/// <param name="groupName">name of the group whom we want to share a file/folder.</param>
		/// <param name="perms">permissions of the shared object.</param>
		public GroupShare ShareWithGroup(string path, string groupName, int perms = -1) {
			if ((perms == -1) || (perms > Convert.ToInt32 (OcsPermission.All)) || (groupName == null) || (groupName.Equals ("")))
				return null;

            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments);

			/*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { shareType = Convert.ToInt32(OcsShareType.Group) });
            parameters.Add(new { path = path });
            if (perms != Convert.ToInt32(OcsPermission.None))
                parameters.Add(new { permissions = perms });
            else
                parameters.Add(new { permissions = Convert.ToInt32(OcsPermission.Read) });
            parameters.Add(new { shareWith = groupName });

			/*request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Group));
			request.AddParameter ("path", path);
			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", perms + "");
			else
				request.AddParameter("permissions", Convert.ToInt32(OcsPermission.Read) + "");
			request.AddParameter ("shareWith", groupName);*/

			//var response = rest.Execute (request).Result;
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            /*var share = new GroupShare();
			share.ShareId = Convert.ToInt32(GetFromData(response.Content, "id"));
            share.TargetPath = path;
            share.Perms = perms;
			share.SharedWith = groupName;*/

            return (GroupShare)response.Result.Data;
        }

		/// <summary>
		/// Checks whether a path is already shared.
		/// </summary>
		/// <returns><c>true</c> if this instance is shared the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">path to the share to be checked.</param>
		public bool IsShared(string path) {
			var result = GetShares (path);
			return result.Count > 0;
		}

		/// <summary>
		/// Gets all shares for the current user when <c>path</c> is not set, otherwise it gets shares for the specific file or folder
		/// </summary>
		/// <returns>array of shares or empty array if the operation failed.</returns>
		/// <param name="path">(optional) path to the share to be checked.</param>
		/// <param name="reshares">(optional) returns not only the shares from	the current user but all shares from the given file.</param>
		/// <param name="subfiles">(optional) returns all shares within	a folder, given that path defines a folder.</param>
		public List<Share> GetShares(string path, OcsBoolParam reshares = OcsBoolParam.None, OcsBoolParam subfiles = OcsBoolParam.None) {
            var urlSegments = GetOcsPath(ocsServiceShare, "shares");
            var request = rest.Request(urlSegments);

			/*var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares") , Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            if ((path != null) && (!path.Equals("")))
                parameters.Add(new { path = path });
            if (reshares == OcsBoolParam.True)
                parameters.Add(new { reshares = true });
            else if (reshares == OcsBoolParam.False)
                parameters.Add(new { reshares = false });
            if (subfiles == OcsBoolParam.True)
                parameters.Add(new { subfiles = true });
            else if (subfiles == OcsBoolParam.False)
                parameters.Add(new { subfiles = false });

			/*if ((path != null) && (!path.Equals("")))
            	request.AddQueryParameter("path", path);
			if (reshares == OcsBoolParam.True)
				request.AddQueryParameter("reshares", "true");
			else if (reshares == OcsBoolParam.False)
				request.AddQueryParameter("reshares", "false");
			if (subfiles == OcsBoolParam.True)
				request.AddQueryParameter("subfiles", "true");
			else if (subfiles == OcsBoolParam.False)
				request.AddQueryParameter("subfiles", "false");*/
			
			//var response = rest.Execute (request).Result;

            request.SetQueryParams(parameters.ToArray());
            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (List<Share>)response.Result.Data;
		}
		#endregion

		#region Users
		/// <summary>
		/// Create a new user with an initial password via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if user was created, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be created.</param>
		/// <param name="initialPassword">password for user being created.</param> 
		public bool CreateUser(string username, string initialPassword) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            parameters.Add(new QueryParameter("userid", username));
            parameters.Add(new QueryParameter("password", initialPassword));
            parameters.Add(new QueryParameter("format", "json"));
            
			/*request.AddParameter ("userid", username);
			request.AddParameter ("password", initialPassword);*/

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<dynamic>();
            response.Wait();

            var content = OCS.JSonDeserialize(response.Result, typeof(User));
            /*var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}*/

			return false;
		}

		/// <summary>
		/// Deletes a user via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if user was deleted, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be deleted.</param>
		public bool DeleteUser(string username) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            //var response = rest.Execute<OCS>(request).Result;

            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Checks a user via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if exists was usered, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be checked.</param>
		public bool UserExists(string username) {
			var result = SearchUsers (username);
			return result.Contains(username);
		}

		/// <summary>
		/// Searches for users via provisioning API.
		/// </summary>
		/// <returns>list of users.</returns>
		/// <param name="username">name of user to be searched for.</param>
		public List<string> SearchUsers(string username) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("?search={0}", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "?search={userid}", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (List<string>)response.Result.Data;
		}

		/// <summary>
		/// Gets the user's attributes.
		/// </summary>
		/// <returns>The user attributes.</returns>
		/// <param name="username">Username.</param>
		public User GetUserAttributes(string username) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            //var response = rest.Execute (request).Result;
            var parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("format", "json"));

            request.SetQueryParams(parameters);
            var response = request.GetJsonAsync<dynamic>();
            response.Wait();

            var content = OCS.JSonDeserialize(response.Result, typeof(User));
            //CheckOcsStatus ();

            return (User)content.Data;
		}

		/// <summary>
		/// Sets a user attribute. See https://doc.owncloud.com/server/7.0EE/admin_manual/configuration_auth_backends/user_provisioning_api.html#users-edituser for reference.
		/// </summary>
		/// <returns><c>true</c>, if user attribute was set, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to modify.</param>
		/// <param name="key">key of the attribute to set.</param>
		/// <param name="value">value to set.</param>
		public bool SetUserAttribute(string username, OCSUserAttributeKey key, string value) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}", Method.PUT);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            var parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("key", OCSUserAttributeKeyName[Convert.ToInt32(key)]));
            parameters.Add(new QueryParameter("value", value ));
            parameters.Add(new QueryParameter("format", "json"));

			/*request.AddParameter ("key", OCSUserAttributeKeyName[Convert.ToInt32(key)]);
			request.AddParameter ("value", value);*/

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PutAsync(new StringContent("")).ReceiveJson<dynamic>();
            response.Wait();

            var content = OCS.JSonDeserialize(response.Result, typeof(User));

            if (content != null) {
                if (content.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (content.Meta.Message, content.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Adds a user to a group.
		/// </summary>
		/// <returns><c>true</c>, if user was added to group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be added.</param>
		/// <param name="groupName">name of group user is to be added to.</param>
		public bool AddUserToGroup(string username, string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/groups", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { groupid = groupName });

			//request.AddParameter ("groupid", groupName);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Get a list of groups associated to a user.
		/// </summary>
		/// <returns>list of groups.</returns>
		/// <param name="username">name of user to list groups.</param>
		public List<string> GetUserGroups (string username) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/groups", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (List<string>)response.Result.Data;
		}

		/// <summary>
		/// Check if a user is in a group.
		/// </summary>
		/// <returns><c>true</c>, if user is in group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user.</param>
		/// <param name="groupName">name of group.</param>
		public bool IsUserInGroup (string username, string groupName) {
			var groups = GetUserGroups (username);
			return groups.Contains (groupName);
		}

		/// <summary>
		/// Removes a user from a group.
		/// </summary>
		/// <returns><c>true</c>, if user was removed from group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be removed.</param>
		/// <param name="groupName">name of group user is to be removed from.</param>
		public bool RemoveUserFromGroup(string username, string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/groups", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { groupid = groupName });

			//request.AddParameter ("groupid", groupName);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Adds a user to a subadmin group.
		/// </summary>
		/// <returns><c>true</c>, if user was added to sub admin group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be added to subadmin group.</param>
		/// <param name="groupName">name of subadmin group.</param>
		public bool AddUserToSubAdminGroup(string username, string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/subadmins", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/subadmins", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { groupid = groupName });

			//request.AddParameter ("groupid", groupName);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Get a list of subadmin groups associated to a user.
		/// </summary>
		/// <returns>list of subadmin groups.</returns>
		/// <param name="username">name of user.</param>
		public List<string> GetUserSubAdminGroups (string username) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/subadmins", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/subadmins", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			try {
				CheckOcsStatus (response);
			} catch (OCSResponseError ocserr) {
				if (ocserr.StatusCode.Equals ("102")) // empty response results in a OCS 102 Error
					return new List<string> ();
			}

            return (List<string>)response.Result.Data;
		}

		/// <summary>
		/// Check if a user is in a subadmin group.
		/// </summary>
		/// <returns><c>true</c>, if user is in sub admin group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user.</param>
		/// <param name="groupNname">name of subadmin group.</param>
		public bool IsUserInSubAdminGroup (string username, string groupNname) {
			var groups = GetUserSubAdminGroups (username);
			return groups.Contains (groupNname);
		}

		/// <summary>
		/// Removes the user from sub admin group.
		/// </summary>
		/// <returns><c>true</c>, if user from sub admin group was removed, <c>false</c> otherwise.</returns>
		/// <param name="username">Username.</param>
		/// <param name="groupName">Group name.</param>
		public bool RemoveUserFromSubAdminGroup(string username, string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "users");
            var request = rest.Request(urlSegments + String.Format("/{0}/subadmins", username));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/subadmins", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);*/
			
            List<Object> parameters = new List<object>();
            parameters.Add(new { groupid = groupName });

            //request.AddParameter ("groupid", groupName);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion

		#region Groups
		/// <summary>
		/// Create a new group via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if group was created, <c>false</c> otherwise.</returns>
		/// <param name="groupName">name of group to be created.</param>
		public bool CreateGroup(string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "groups");
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { groupid = groupName });

			//request.AddParameter ("groupid", groupName);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Deletes the group.
		/// </summary>
		/// <returns><c>true</c>, if group was deleted, <c>false</c> otherwise.</returns>
		/// <param name="groupName">Group name.</param>
		public bool DeleteGroup(string groupName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "groups");
            var request = rest.Request(urlSegments + String.Format("/{0}", groupName));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups") + "/{groupid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("groupid", groupName);*/

			//var response = rest.Execute<OCS>(request).Result;
			
            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Checks a group via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if group exists, <c>false</c> otherwise.</returns>
		/// <param name="groupName">name of group to be checked.</param>
		public bool GroupExists(string groupName) {
            var results = SearchGroups(groupName);
            return results.Contains(groupName);
		}

        /// <summary>
		/// Searches for groups via provisioning API.
		/// </summary>
		/// <returns>list of groups.</returns>
		/// <param name="name">name of group to be searched for.</param>
		public List<string> SearchGroups(string name)
        {
            var urlSegments = GetOcsPath(ocsServiceCloud, "groups");
            var request = rest.Request(urlSegments + String.Format("?search={0}", name));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups") + "?search={groupid}", Method.GET);
            request.AddHeader("OCS-APIREQUEST", "true");

            request.AddUrlSegment("groupid", name);*/

            //var response = rest.Execute(request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

            CheckOcsStatus(response);

            return (List<string>)response.Result.Data;
        }
        #endregion

        #region Config
        /// <summary>
        /// Returns ownCloud config information.
        /// </summary>
        /// <returns>The config.</returns>
        public Config GetConfig() {
            var urlSegments = GetOcsPath("", "config");
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath("", "config"), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);
            /*
			Config cfg = new Config ();
			cfg.Contact = GetFromData (response.Content, "contact");
			cfg.Host = GetFromData (response.Content, "host");
			cfg.Ssl  = GetFromData (response.Content, "ssl");
			cfg.Version = GetFromData (response.Content, "version");
			cfg.website = GetFromData (response.Content, "website");
            */

            return (Config)response.Result.Data;
		}
		#endregion

		#region Application attributes
		/// <summary>
		/// Returns an application attribute
		/// </summary>
		/// <returns>App Attribute List.</returns>
		/// <param name="app">application id.</param>
		/// <param name="key">attribute key or None to retrieve all values for the given application.</param>
		public List<AppAttribute> GetAttribute(string app = "", string key = "") {
			var path = "getattribute";
			if (!app.Equals ("")) {
				path += "/" + app;
				if (!key.Equals (""))
					path += "/" + WebUtility.UrlEncode (key);
			}

            var urlSegments = GetOcsPath(ocsServiceData, path);
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (List<AppAttribute>)response.Result.Data;
		}

		/// <summary>
		/// Sets an application attribute.
		/// </summary>
		/// <returns><c>true</c>, if attribute was set, <c>false</c> otherwise.</returns>
		/// <param name="app">application id.</param>
		/// <param name="key">key of the attribute to set.</param>
		/// <param name="value">value to set.</param>
		public bool SetAttribute(string app, string key, string value) {
			var path = "setattribute" + "/" + app + "/" + WebUtility.UrlEncode (key);

            var urlSegments = GetOcsPath(ocsServiceData, path);
            var request = rest.Request(urlSegments);

			/*var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            List<Object> parameters = new List<object>();
            parameters.Add(new { value = value });

			//request.AddParameter ("value", value);

			//var response = rest.Execute<OCS>(request).Result;
			
            request.SetQueryParams(parameters.ToArray());
            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Deletes an application attribute.
		/// </summary>
		/// <returns><c>true</c>, if attribute was deleted, <c>false</c> otherwise.</returns>
		/// <param name="app">application id.</param>
		/// <param name="key">key of the attribute to delete.</param>
		public bool DeleteAttribute(string app, string key) {
			var path = "deleteattribute" + "/" + app + "/" + WebUtility.UrlEncode (key);

            var urlSegments = GetOcsPath(ocsServiceData, path);
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            //var response = rest.Execute<OCS>(request).Result;

            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion

		#region Apps
		/// <summary>
		/// List all enabled apps through the provisioning api.
		/// </summary>
		/// <returns>a list of apps and their enabled state.</returns>
		public List<string> GetApps() {
            var urlSegments = GetOcsPath(ocsServiceCloud, "apps");
            var request = rest.Request(urlSegments);

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps"), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (List<string>)response.Result.Data;
		}

		/// <summary>
		/// Gets information about the specified app.
		/// </summary>
		/// <returns>App information.</returns>
		/// <param name="appName">App name.</param>
		public AppInfo GetApp(string appName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "apps");
            var request = rest.Request(urlSegments + String.Format("/{0}", appName));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps") + "/{appid}", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");
			request.AddUrlSegment ("appid", appName);*/

            //var response = rest.Execute (request).Result;

            var response = request.GetJsonAsync<OCS>();
            response.Wait();

			CheckOcsStatus (response);

            return (AppInfo)response.Result.Data;
		}

		/// <summary>
		/// Enable an app through provisioning_api.
		/// </summary>
		/// <returns><c>true</c>, if app was enabled, <c>false</c> otherwise.</returns>
		/// <param name="appName">Name of app to be enabled.</param>
		public bool EnableApp(string appName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "apps");
            var request = rest.Request(urlSegments + String.Format("/{0}", appName));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps") + "/{appid}", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("appid", appName);*/

            //var response = rest.Execute<OCS>(request).Result;

            var response = request.PostAsync(new StringContent("")).ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Disable an app through provisioning_api
		/// </summary>
		/// <returns><c>true</c>, if app was disabled, <c>false</c> otherwise.</returns>
		/// <param name="appName">Name of app to be disabled.</param>
		public bool DisableApp(string appName) {
            var urlSegments = GetOcsPath(ocsServiceCloud, "apps");
            var request = rest.Request(urlSegments + String.Format("/{0}", appName));

            /*var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps") + "/{appid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("appid", appName);*/

            //var response = rest.Execute<OCS>(request).Result;

            var response = request.DeleteAsync().ReceiveJson<OCS>();
            response.Wait();

            if (response.Result != null) {
                if (response.Result.Meta.StatusCode == 100)
					return true;
				else
                    throw new OCSResponseError (response.Result.Meta.Message, response.Result.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion
		#endregion

		#region Url Handling
		/// <summary>
		/// Gets the full URI.
		/// </summary>
		/// <returns>The URI.</returns>
		/// <param name="path">remote Path.</param>
		private Uri GetUri(string path) {
			return new Uri (this.url + path);
		}

		/// <summary>
		/// Gets the DAV request URI.
		/// </summary>
		/// <returns>The DAV URI.</returns>
		/// <param name="path">remote Path.</param>
		private Uri GetDavUri(string path) {
			return new Uri (this.url + "/" + davpath + path);
		}

		/// <summary>
		/// Gets the remote path for OCS API.
		/// </summary>
		/// <returns>The ocs path.</returns>
		/// <param name="service">Service.</param>
		/// <param name="action">Action.</param>
		private string GetOcsPath(string service, string action) {
			var slash = (!service.Equals("")) ? "/" : "";
			return service + slash + action;
		}
		#endregion

		#region OCS Response parsing
		/// <summary>
		/// Get element value from OCS Meta.
		/// </summary>
		/// <returns>Element value.</returns>
		/// <param name="response">XML OCS response.</param>
		/// <param name="elementName">XML Element name.</param>
		private string GetFromMeta(string response, string elementName) {
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("meta"))) {
				var node = data.Element(XName.Get(elementName));
				if (node != null)
					return node.Value;
			}

			return null;
		}

		/// <summary>
		/// Get element value from OCS Data.
		/// </summary>
		/// <returns>Element value.</returns>
		/// <param name="response">XML OCS response.</param>
		/// <param name="elementName">XML Element name.</param>
		private string GetFromData(string response, string elementName) {
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("data"))) {
				var node = data.Element(XName.Get(elementName));
				if (node != null)
					return node.Value;
			}

			return null;
		}

		/// <summary>
		/// Gets the data element values.
		/// </summary>
		/// <returns>The data elements.</returns>
		/// <param name="response">XML OCS Response.</param>
		private List<string> GetDataElements(string response) {
			List<string> result = new List<string> ();
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("data"))) {
				foreach (XElement node in data.Descendants(XName.Get("element")) ){
					result.Add(node.Value);
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the share list from a OCS Data response.
		/// </summary>
		/// <returns>The share list.</returns>
		/// <param name="response">XML OCS Response.</param>
        private List<Share> GetShareList(string response)
        {
            List<Share> shares = new List<Share>();
            XDocument xdoc = XDocument.Parse(response);

            foreach (XElement data in xdoc.Descendants(XName.Get("element")))
            {
				Share share = null;
                var node = data.Element(XName.Get("share_type"));
                if (node != null)
                {
					#region Share Type
					var shareType = Convert.ToInt32 (node.Value);
					if (shareType == Convert.ToInt32 (OcsShareType.Link))
						share = new PublicShare ();
					else if (shareType == Convert.ToInt32 (OcsShareType.User))
						share = new UserShare ();
					else if (shareType == Convert.ToInt32 (OcsShareType.Group))
						share = new GroupShare ();
					else
						share = new Share ();
					share.AdvancedProperties = new AdvancedShareProperties ();
					#endregion

					#region General Properties
					node = data.Element(XName.Get("id"));
					if (node != null)
						share.ShareId = Convert.ToInt32(node.Value);

					node = data.Element(XName.Get("file_target"));
					if (node != null)
						share.TargetPath = node.Value;

					node = data.Element(XName.Get("permissions"));
					if (node != null)
						share.Perms = Convert.ToInt32(node.Value);
					#endregion

					#region Advanced Properties
					node = data.Element(XName.Get("item_type"));
					if (node != null)
						share.AdvancedProperties.ItemType = node.Value;
					
					node = data.Element(XName.Get("item_source"));
					if (node != null)
						share.AdvancedProperties.ItemSource = node.Value;

					node = data.Element(XName.Get("parent"));
					if (node != null)
						share.AdvancedProperties.Parent = node.Value;

					node = data.Element(XName.Get("file_source"));
					if (node != null)
						share.AdvancedProperties.FileSource = node.Value;

					node = data.Element(XName.Get("stime"));
					if (node != null)
						share.AdvancedProperties.STime = node.Value;

					node = data.Element(XName.Get("expiration"));
					if (node != null)
						share.AdvancedProperties.Expiration = node.Value;

					node = data.Element(XName.Get("mail_send"));
					if (node != null)
						share.AdvancedProperties.MailSend = node.Value;

					node = data.Element(XName.Get("uid_owner"));
					if (node != null)
						share.AdvancedProperties.Owner = node.Value;

					node = data.Element(XName.Get("storage_id"));
					if (node != null)
						share.AdvancedProperties.StorageId = node.Value;

					node = data.Element(XName.Get("storage"));
					if (node != null)
						share.AdvancedProperties.Storage = node.Value;

					node = data.Element(XName.Get("file_parent"));
					if (node != null)
						share.AdvancedProperties.FileParent = node.Value;

					node = data.Element(XName.Get("share_with_displayname"));
					if (node != null)
						share.AdvancedProperties.ShareWithDisplayname = node.Value;

					node = data.Element(XName.Get("displayname_owner"));
					if (node != null)
						share.AdvancedProperties.DisplaynameOwner = node.Value;
					#endregion

					#region ShareType specific
					if (shareType == Convert.ToInt32(OcsShareType.Link)) {
						node = data.Element(XName.Get("url"));
						if (node != null)
							((PublicShare)share).Url = node.Value;

						node = data.Element(XName.Get("token"));
						if (node != null)
							((PublicShare)share).Token = node.Value;
					}
					else if (shareType == Convert.ToInt32(OcsShareType.User)) {
						node = data.Element(XName.Get("share_with"));
						if (node != null)
							((UserShare)share).SharedWith = node.Value;
					}
					else if (shareType == Convert.ToInt32(OcsShareType.Group)) {
						node = data.Element(XName.Get("share_with"));
						if (node != null)
							((GroupShare)share).SharedWith = node.Value;
					}
					#endregion

					shares.Add (share);
                }
            }

            return shares;
        }

		/// <summary>
		/// Checks the validity of the OCS Request. If invalid a exception is thrown.
		/// </summary>
		/// <param name="response">OCS Response.</param>
        private void CheckOcsStatus(Task<OCS> response) {
            if (response.Result == null)
                throw new ResponseError(response.Result.Meta.Message);
			else {
                var ocsStatus = response.Result.Meta.Status;
				if (ocsStatus == null)
					throw new ResponseError ("Empty response");
				if (!ocsStatus.Equals ("100"))
                    throw new OCSResponseError (GetFromMeta (response.Result.Meta.Message, ocsStatus));
			}
		}

		/// <summary>
		/// Returns a list of application attributes.
		/// </summary>
		/// <returns>List of application attributes.</returns>
		/// <param name="response">XML OCS Response.</param>
		private List<AppAttribute> GetAttributeList(string response) {
			List<AppAttribute> result = new List<AppAttribute> ();
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("data"))) {
				foreach (XElement element in data.Descendants(XName.Get("element")) ){
					AppAttribute attr = new AppAttribute ();

					var node = element.Element(XName.Get("app"));
					if (node != null)
						attr.App = node.Value;

					node = element.Element(XName.Get("key"));
					if (node != null)
						attr.Key = node.Value;

					node = element.Element(XName.Get("value"));
					if (node != null)
						attr.value = node.Value;

					result.Add (attr);
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the user attributes from a OCS XML Response.
		/// </summary>
		/// <returns>The user attributes.</returns>
		/// <param name="response">OCS XML Response.</param>
		private User GetUser(string response) {
			var user = new User ();
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("data"))) {
				var node = data.Element(XName.Get("displayname"));
				if (node != null)
					user.DisplayName = node.Value;

				node = data.Element(XName.Get("email"));
				if (node != null)
					user.EMail = node.Value;

				node = data.Element(XName.Get("enabled"));
				if (node != null)
					user.Enabled = (node.Value.Equals ("true")) ? true : false;
				
				foreach (XElement element in data.Descendants(XName.Get("quota")) ){
					var quota = new Quota ();

					node = element.Element(XName.Get("free"));
					if (node != null)
						quota.Free = double.Parse(node.Value, CultureInfo.InvariantCulture);

                    node = element.Element(XName.Get("used"));
					if (node != null)
						quota.Used = double.Parse(node.Value, CultureInfo.InvariantCulture);

                    node = element.Element(XName.Get("total"));
					if (node != null)
						quota.Total = double.Parse(node.Value, CultureInfo.InvariantCulture);

                    node = element.Element(XName.Get("relative"));
                    if (node != null)
                        quota.Relative = double.Parse(node.Value, CultureInfo.InvariantCulture);

                    user.Quota = quota;
				}
			}

			return user;
		}

		private AppInfo GetAppInfo(string response) {
			AppInfo app = new AppInfo ();
			XDocument xdoc = XDocument.Parse(response);

			foreach (XElement data in xdoc.Descendants(XName.Get("data"))) {
				var node = data.Element (XName.Get ("id"));
				if (node != null)
					app.Id = node.Value;

				node = data.Element (XName.Get ("name"));
				if (node != null)
					app.Name = node.Value;

				node = data.Element (XName.Get ("description"));
				if (node != null)
					app.Description = node.Value;

				node = data.Element (XName.Get ("licence"));
				if (node != null)
					app.Licence = node.Value;

				node = data.Element (XName.Get ("author"));
				if (node != null)
					app.Author = node.Value;

				node = data.Element (XName.Get ("requiremin"));
				if (node != null)
					app.RequireMin = node.Value;

				node = data.Element (XName.Get ("shipped"));
				if (node != null)
					app.Shipped = (node.Value.Equals ("true")) ? true : false;

				node = data.Element (XName.Get ("standalone"));
				if (node != null)
					app.Standalone = true;
				else
					app.Standalone = false;

				node = data.Element (XName.Get ("default_enable"));
				if (node != null)
					app.DefaultEnable = true;
				else
					app.DefaultEnable = false;

				node = data.Element (XName.Get ("types"));
				if (node != null)
					app.Types = XmlElementsToList (node);

				node = data.Element (XName.Get ("remote"));
				if (node != null)
					app.Remote = XmlElementsToDict (node);

				node = data.Element (XName.Get ("documentation"));
				if (node != null)
					app.Documentation = XmlElementsToDict (node);

				node = data.Element (XName.Get ("info"));
				if (node != null)
					app.Info = XmlElementsToDict (node);

				node = data.Element (XName.Get ("public"));
				if (node != null)
					app.Public = XmlElementsToDict (node);
			}

			return app;
		}

		/// <summary>
		/// Returns the elements of a XML Element as a List.
		/// </summary>
		/// <returns>The elements as list.</returns>
		/// <param name="element">XML Element.</param>
		private List<string> XmlElementsToList(XElement element) {
			List<string> result = new List<string> ();

			foreach (XElement node in element.Descendants(XName.Get("element"))) {
				result.Add (node.Value);
			}

			return result;
		}

		/// <summary>
		/// Returns the elements of a XML Element as a Dictionary.
		/// </summary>
		/// <returns>The elements as dictionary.</returns>
		/// <param name="element">XML Element.</param>
		private Dictionary<string, string> XmlElementsToDict(XElement element) {
			Dictionary<string, string> result = new Dictionary<string, string>();

			foreach (XElement node in element.Descendants()) {
				result.Add (node.Name.ToString(), node.Value);
			}

			return result;
		}
        #endregion

        #region Helpers
        /// <summary>
        /// Provides the string values for the OCSUserAttributeKey enum
        /// </summary>
        public static string[] OCSUserAttributeKeyName = new string[] {
        "display",
        "quota",
        "password",
        "email"
        };
        #endregion
    }
}

