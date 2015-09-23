using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

using RestSharp;
using RestSharp.Authenticators;
using WebDav;
using WebClient;

using owncloudsharp.Exceptions;
using owncloudsharp.Types;

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
		private RestClient rest;
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
			this.rest = new RestClient ();
			// Set the base path as the OCS API root
			this.rest.BaseUrl = new Uri (url + "/" + ocspath);
			// Configure RestSharp for BasicAuth
			this.rest.Authenticator = new HttpBasicAuthenticator (user_id, password);

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
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "remote_shares"), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");
			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}

		/// <summary>
		/// Accepts a remote share
		/// </summary>
		/// <returns><c>true</c>, if remote share was accepted, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		public bool AcceptRemoteShare(int shareId) {
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "remote_shares") + "/{id}", Method.POST);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Declines a remote share.
		/// </summary>
		/// <returns><c>true</c>, if remote share was declined, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		public bool DeclineRemoteShare(int shareId) {
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "remote_shares") + "/{id}", Method.DELETE);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares") + "/{id}", Method.DELETE);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Updates a given share
		/// </summary>
		/// <returns><c>true</c>, if share was updated, <c>false</c> otherwise.</returns>
		/// <param name="shareId">Share identifier.</param>
		/// <param name="perms">(optional) update permissions.</param>
		/// <param name="password">(optional) updated password for public link Share.</param>
		/// <param name="public_upload">(optional) If set to <c>true</c> enables public upload for public shares.</param>
		public bool UpdateShare(int shareId, int perms = -1, string password = null, OcsBoolParam public_upload = OcsBoolParam.None) {
			if ((perms == Convert.ToInt32(OcsPermission.None)) && (password == null) && (public_upload == OcsBoolParam.None))
				return false;
			
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares") + "/{id}", Method.PUT);
			request.AddUrlSegment("id", "" + shareId);
			request.AddHeader("OCS-APIREQUEST", "true");

			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", Convert.ToInt32(perms) + "");
			if (password != null)
				request.AddParameter ("password", password);
			if (public_upload == OcsBoolParam.True)
				request.AddParameter ("publicUpload", "true");
			else if (public_upload == OcsBoolParam.False)
				request.AddParameter ("publicUpload", "false");

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Link));
			request.AddParameter ("path", path);

			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", Convert.ToInt32(perms) + "");
			if (password != null)
				request.AddParameter ("password", password);
			if (public_upload == OcsBoolParam.True)
				request.AddParameter ("publicUpload", "true");
			else if (public_upload == OcsBoolParam.False)
				request.AddParameter ("publicUpload", "false");

			var response = rest.Execute (request);

			PublicShare share = new PublicShare ();
			share.ShareId = GetFromData(response.Content, "id");
			share.Url = GetFromData(response.Content, "url");
			share.Token = GetFromData(response.Content, "token");
			share.TargetPath = path;

			return share;
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

			var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			if (remoteUser == OcsBoolParam.True)
				request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Remote));
			else
				request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.User));
			request.AddParameter ("path", path);
			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", perms + "");
			else
				request.AddParameter("permissions", Convert.ToInt32(OcsPermission.Read) + "");
			request.AddParameter ("shareWith", username);

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Implement parse to UserShare
			return new UserShare();
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

			var request = new RestRequest(GetOcsPath(ocsServiceShare, "shares"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddParameter ("shareType", Convert.ToInt32 (OcsShareType.Group));
			request.AddParameter ("path", path);
			if (perms != Convert.ToInt32(OcsPermission.None))
				request.AddParameter("permissions", perms + "");
			else
				request.AddParameter("permissions", Convert.ToInt32(OcsPermission.Read) + "");
			request.AddParameter ("shareWith", groupName);

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Implement parse to GroupShare
			return new GroupShare();
		}

		/// <summary>
		/// Checks whether a path is already shared.
		/// </summary>
		/// <returns><c>true</c> if this instance is shared the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">path to the share to be checked.</param>
		public object IsShared(string path) {
			var result = GetShares (path);
			// TODO: Perform query
			return result;
		}

		/// <summary>
		/// Returns array of shares.
		/// </summary>
		/// <returns>array of shares or empty array if the operation failed.</returns>
		/// <param name="path">path to the share to be checked.</param>
		/// <param name="reshares">(optional) returns not only the shares from	the current user but all shares from the given file.</param>
		/// <param name="subfiles">(optional) returns all shares within	a folder, given that path defines a folder.</param>
		public object GetShares(string path, OcsBoolParam reshares = OcsBoolParam.None, OcsBoolParam subfiles = OcsBoolParam.None) {
			var request = new RestRequest(GetOcsPath(ocsServiceShare, "") , Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			if (reshares == OcsBoolParam.True)
				request.AddParameter ("reshares", "true");
			else if (reshares == OcsBoolParam.False)
				request.AddParameter ("reshares", "false");
			if (subfiles == OcsBoolParam.True)
				request.AddParameter ("subfiles", "true");
			else if (subfiles == OcsBoolParam.False)
				request.AddParameter ("subfiles", "false");
			
			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse results
			return content;
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
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddParameter ("userid", username);
			request.AddParameter ("password", initialPassword);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Deletes a user via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if user was deleted, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be deleted.</param>
		public bool DeleteUser(string username) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Checks a user via provisioning API.
		/// </summary>
		/// <returns>The exists.</returns>
		/// <param name="username">name of user to be checked.</param>
		public object UserExists(string username) {
			var result = SearchUsers (username);
			// TODO: Implement query
			return result;
		}

		/// <summary>
		/// Searches for users via provisioning API.
		/// </summary>
		/// <returns>list of users.</returns>
		/// <param name="username">name of user to be searched for.</param>
		public object SearchUsers(string username) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "?search={userid}", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}

		/// <summary>
		/// Sets a user attribute.
		/// </summary>
		/// <returns><c>true</c>, if user attribute was set, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to modify.</param>
		/// <param name="key">key of the attribute to set.</param>
		/// <param name="value">value to set.</param>
		public bool SetUserAttribute(string username, string key, string value) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}", Method.PUT);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);
			request.AddParameter ("key", key);
			request.AddParameter ("value", value);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);
			request.AddParameter ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Get a list of groups associated to a user.
		/// </summary>
		/// <returns>list of groups.</returns>
		/// <param name="username">name of user to list groups.</param>
		public object GetUserGroups (string username) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}

		/// <summary>
		/// Check if a user is in a group.
		/// </summary>
		/// <returns><c>true</c>, if user is in group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user.</param>
		/// <param name="groupName">name of group.</param>
		public bool UserIsInGroup (string username, string groupName) {
			var groups = GetUserGroups (username);
			// TODO: Implement query
			return false;
		}

		/// <summary>
		/// Removes a user from a group.
		/// </summary>
		/// <returns><c>true</c>, if user was removed from group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user to be removed.</param>
		/// <param name="groupName">name of group user is to be removed from.</param>
		public bool RemoveUserFromGroup(string username, string groupName) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/groups", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);
			request.AddParameter ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/subadmins", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);
			request.AddParameter ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Get a list of subadmin groups associated to a user.
		/// </summary>
		/// <returns>list of subadmin groups.</returns>
		/// <param name="username">name of user.</param>
		public object GetUserSubAdminGroups (string username) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "users") + "/{userid}/subadmins", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("userid", username);

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}

		/// <summary>
		/// Check if a user is in a subadmin group.
		/// </summary>
		/// <returns><c>true</c>, if user is in sub admin group, <c>false</c> otherwise.</returns>
		/// <param name="username">name of user.</param>
		/// <param name="groupNname">name of subadmin group.</param>
		public bool UserIsInSubAdminGroup (string username, string groupNname) {
			var groups = GetUserSubAdminGroups (username);
			// TODO: Implement query
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
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups"), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddParameter ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Deletes the group.
		/// </summary>
		/// <returns><c>true</c>, if group was deleted, <c>false</c> otherwise.</returns>
		/// <param name="groupName">Group name.</param>
		public bool DeleteGroup(string groupName) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups") + "/{groupid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Checks a group via provisioning API.
		/// </summary>
		/// <returns><c>true</c>, if group exists, <c>false</c> otherwise.</returns>
		/// <param name="groupName">name of group to be checked.</param>
		public bool GroupExists(string groupName) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "groups") + "?search={groupid}", Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("groupid", groupName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion

		#region Config
		/// <summary>
		/// Returns ownCloud config information.
		/// </summary>
		/// <returns>The config.</returns>
		public object GetConfig() {
			var request = new RestRequest(GetOcsPath("", "config"), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}
		#endregion

		#region Application attributes
		/// <summary>
		/// Returns an application attribute
		/// </summary>
		/// <returns>The attribute.</returns>
		/// <param name="app">application id.</param>
		/// <param name="key">attribute key or None to retrieve all values for the given application.</param>
		public object GetAttribute(string app = "", string key = "") {
			var path = "getattribute";
			if (!app.Equals ("")) {
				path += "/" + app;
				if (!key.Equals (""))
					path += "/" + WebUtility.UrlEncode (key);
			}

			var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
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

			var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");
			request.AddParameter ("value", value);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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

			var request = new RestRequest(GetOcsPath(ocsServiceData, path), Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}
		#endregion

		#region Apps
		/// <summary>
		/// List all enabled apps through the provisioning api.
		/// </summary>
		/// <returns>a list of apps and their enabled state.</returns>
		public object GetApp() {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps"), Method.GET);
			request.AddHeader("OCS-APIREQUEST", "true");

			var response = rest.Execute (request);
			var content = response.Content; 
			// TODO: Parse response
			return content;
		}

		/// <summary>
		/// Enable an app through provisioning_api.
		/// </summary>
		/// <returns><c>true</c>, if app was enabled, <c>false</c> otherwise.</returns>
		/// <param name="appName">Name of app to be enabled.</param>
		public bool EnableApp(string appName) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps") + "/{appid}", Method.POST);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("appid", appName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
			}

			return false;
		}

		/// <summary>
		/// Disable an app through provisioning_api
		/// </summary>
		/// <returns><c>true</c>, if app was disabled, <c>false</c> otherwise.</returns>
		/// <param name="appName">Name of app to be disabled.</param>
		public bool DisableApp(string appName) {
			var request = new RestRequest(GetOcsPath(ocsServiceCloud, "apps") + "/{appid}", Method.DELETE);
			request.AddHeader("OCS-APIREQUEST", "true");

			request.AddUrlSegment ("appid", appName);

			var response = rest.Execute<OCS>(request);
			if (response.Data != null) {
				if (response.Data.Meta.StatusCode == 100)
					return true;
				else if (response.Data.Meta.StatusCode > 900)
					throw new OCSResponseError (response.Data.Meta.Message, response.Data.Meta.StatusCode + "");
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
		#endregion
	}
}

