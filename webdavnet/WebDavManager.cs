// --------------------------------
// <copyright file="WebDavManager.cs" company="Thomas Loehlein">
//     WebDavNet - A WebDAV client
//     Copyright (C) 2009 - Thomas Loehlein
//     This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License along with this program; if not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Thomas Loehlein</author>
// <email>thomas.loehlein@gmail.com</email>
// ---------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WebDav
{
    /// <summary>
    /// Base manager class for handling all WebDav purpose.
    /// </summary>
    public class WebDavManager
    {
        #region PRIVATE PROPERTIES
		private WebClient client;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavManager"/> class.
        /// </summary>
        public WebDavManager()
        {
			client = new WebClient ();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavManager"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public WebDavManager(WebDavCredential credential)
        {
			client = new WebClient ();
            Credential = credential;
        }
        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Gets or sets the credential.
        /// </summary>
        /// <value>The credential.</value>
        public WebDavCredential Credential
        { get; set; }

        /// <summary>
        /// Gets or sets the proxy.
        /// </summary>
        /// <value>The proxy.</value>
        public IWebProxy Proxy
        { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout
        { get; set; }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Deletes the resource behind the specified Url.
        /// </summary>
        /// <param name="url">The Url.</param>
        public void Delete(string url)
		{
        	Delete(new Uri(url));
        }

        /// <summary>
        /// Deletes the resource behind the specified Uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        public void Delete(Uri uri)
        {
			HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.Delete);
            HttpResponseMessage webResponse;


        	try
            {
                webResponse = client.Execute(webRequest);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'Delete'" + Environment.NewLine + uri);

                return;
            }
        }

        /// <summary>
        /// Creates a directory on the given Url address.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <returns></returns>
        public bool CreateDirectory(string url)
        {
        	return CreateDirectory(new Uri(url));
        }

        /// <summary>
        /// Creates a directory on the given Uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <returns></returns>
        public bool CreateDirectory(Uri uri)
        {
        	HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.MkCol);
            HttpResponseMessage webResponse;

			try
            {
                webResponse = client.Execute(webRequest);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return false;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'CreateDirectory'" + Environment.NewLine + uri);

                return false;
            }
            
            // Return true if StatusCode is Created
            return webResponse.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Copies the resource of the specified source Url to the target Url.
        /// </summary>
        /// <param name="sourceUrl">The source Url.</param>
        /// <param name="targetUrl">The target Url.</param>
        /// <returns></returns>
        public bool Copy(string sourceUrl, string targetUrl)
        {
        	return Copy(new Uri(sourceUrl), new Uri(targetUrl));
        }

        /// <summary>
        /// Copies the resource of the specified source Uri to the target Uri.
        /// </summary>
        /// <param name="sourceUri">The source Uri.</param>
        /// <param name="targetUri">The target Uri.</param>
        /// <returns></returns>
        public bool Copy(Uri sourceUri, Uri targetUri)
        {
        	HttpRequestMessage webRequest = GetBaseRequest(sourceUri, WebMethod.Copy);
            HttpResponseMessage webResponse;

            webRequest.Headers.Add("Destination", targetUri.ToString());
        	
        	// TODO: Handle overwrite
        	//_WebRequest.Headers.Add("Overwrite", "F") // No Overwrite
        	
            try
            {
                webResponse = client.Execute(webRequest);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return false;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'Copy'" + Environment.NewLine + sourceUri);

                return false;
            }

            return webResponse.StatusCode == HttpStatusCode.Created || webResponse.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Moves the resource of the specified source Url to the target Url.
        /// </summary>
        /// <param name="sourceUrl">The source Url.</param>
        /// <param name="targetUrl">The target Url.</param>
        /// <returns></returns>
        public bool Move(string sourceUrl, string targetUrl)
        {
        	return Move(new Uri(sourceUrl), new Uri(targetUrl));
        }

        /// <summary>
        /// Moves the resource of the specified source Uri to the target Uri.
        /// </summary>
        /// <param name="sourceUri">The source Uri.</param>
        /// <param name="targetUri">The target Uri.</param>
        /// <returns></returns>
        public bool Move(Uri sourceUri, Uri targetUri)
        {
        	HttpRequestMessage webRequest = GetBaseRequest(sourceUri, WebMethod.Move);
            HttpResponseMessage webResponse;

        	webRequest.Headers.Add("Destination", targetUri.ToString());
        	
        	// TODO: Handle overwrite take a look at http://www.ietf.org/rfc/rfc2518.txt first
        	//_WebRequest.Headers.Add("Overwrite", "F") // No Overwrite
        	//_WebRequest.Headers.Add("Overwrite", "T") // ???
        	
			try
            {
                webResponse = client.Execute(webRequest);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return false;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'Move'" + Environment.NewLine + sourceUri);

                return false;
            }
            
            return webResponse.StatusCode == HttpStatusCode.Created || webResponse.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Checks if a resource exists on the specified Url.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <returns></returns>
        public bool Exists(string url)
        {
        	return Exists(new Uri(url));
        }

        /// <summary>
        /// Checks if a resource exists on the specified Uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <returns></returns>
        public bool Exists(Uri uri)
        {
        	HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.Head);
            HttpResponseMessage webResponse;

			try
            {
                webResponse = client.Execute(webRequest);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return false;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'Exists'" + Environment.NewLine + uri);

                return false;
            }
            
            return webResponse.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Lists all resources on the specified Url.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <returns></returns>
        public List<WebDavResource> List(string url)
        {
        	return List(new Uri(url));
        }

        /// <summary>
        /// Lists all resources on the specified Uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <returns></returns>
        public List<WebDavResource> List(Uri uri)
        {
        	List<WebDavResource> listResource;
        	
        	HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.PropFind);
            HttpResponseMessage webResponse;
        	
        	// Retrieve only the requested folder
        	webRequest.Headers.Add("Depth", "1");
        	
			try
            {
                webResponse = client.Execute(webRequest);
				listResource = ExtractResources(webResponse.Content.ReadAsStreamAsync().Result, uri.AbsolutePath);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return new List<WebDavResource>();
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
				Debug.WriteLine("Empty WebResponse @'List'" + Environment.NewLine + uri);

                return new List<WebDavResource>();
            }
            
            return listResource;
        }
        
        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="url">The Url.</param>
		/// <param name="localStream">The local file.</param>
		/// <param name="contentType">The files content type.</param>
		/// <returns>true on success, false on error</returns>
		public bool UploadFile(string url, Stream localStream, string contentType)
        {
			return UploadFile(new Uri(url), localStream, contentType);
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="uri">The Uri.</param>
		/// <param name="localStream">The local file.</param>
		/// <param name="contentType">The files content type.</param>
		/// <returns>true on success, false on error</returns>
		public bool UploadFile(Uri uri, Stream localStream, string contentType)
        {
			HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.Put);
            HttpResponseMessage webResponse;
            

            try
			{
				StreamContent content = new StreamContent(localStream);
				content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
				content.Headers.ContentLength = localStream.Length;
				webRequest.Content = content;
                webResponse = client.Execute(webRequest);
                
				Debug.WriteLine(webResponse.StatusCode.ToString());
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);
                
				return false;
            }
            
            return true;
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="url">The Url.</param>
		/// <returns>File contents.</returns>
        public Stream DownloadFile(string url)
        {
            return DownloadFile(new Uri(url));
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="uri">The Uri.</param>
		/// <returns>File contents.</returns>
        public Stream DownloadFile(Uri uri)
        {
            HttpRequestMessage webRequest = GetBaseRequest(uri, WebMethod.Get);
            HttpResponseMessage webResponse;

			MemoryStream remoteStream = new MemoryStream();

            try
            {
                webResponse = client.Execute(webRequest);
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
        #endregion

        #region PRIVATE METHODS
		private HttpRequestMessage GetBaseRequest(Uri uri, HttpMethod method)
        {
			HttpRequestMessage webRequest = new HttpRequestMessage (method, uri);

            // Set the credentials if available
			if ((Credential != null) && (client.Credentials == null))
            {
				client.Credentials = Credential;
            }
            
            // Set the proxy if available
            if (Proxy != null)
            {
                client.Proxy = Proxy;
            }

            // Set default headers
            webRequest.Headers.Add("Pragma", "no-cache");

			// Set the request timeout
			if (Timeout < 1)
				Timeout = 30000; // At least 30 Seconds

			client.Timeout = new TimeSpan (0, 0, 0, 0, Timeout);

            // TODO: Check if PreAuthenticate is necessary
            // Set PreAuthenticate to assimilate authentication from the plain HEAD request
            //_WebRequest.PreAuthenticate = true;

            return webRequest;
        }

		private static List<WebDavResource> ExtractResources(Stream strm, string rootpath)
        {
            List<WebDavResource> webDavResources = new List<WebDavResource>();

            try
            {
				TextReader treader = new StreamReader (strm);
				string xml = treader.ReadToEnd ();
				treader.Dispose ();

				XDocument xdoc = XDocument.Parse(xml);

				foreach (XElement element in xdoc.Descendants(XName.Get("response", "DAV:")))
                {
                    WebDavResource resource = new WebDavResource();

                    // Do not add hidden files
                    // Hidden files cannot be downloaded from the IIs
                    // For further information see http://support.microsoft.com/kb/216803/

					var prop = element.Descendants(XName.Get("prop", "DAV:")).FirstOrDefault();

					var node = prop.Element(XName.Get("ishidden", "DAV:"));
					if ((node != null) && (node.Value == "1"))
						continue;
					
					node = prop.Element(XName.Get("displayname", "DAV:"));
					if (node != null)
						resource.Name = node.Value;
					
					node = element.Element(XName.Get("href", "DAV:"));
					if (node != null) {
						Uri href;

						if (Uri.TryCreate(node.Value, UriKind.Absolute, out href))
							resource.Uri = href;
					}

					node = prop.Element(XName.Get("getcontentlength", "DAV:"));
					if (node != null)
						resource.Size = int.Parse(node.Value, CultureInfo.CurrentCulture);

					node = prop.Element(XName.Get("creationdate", "DAV:"));
					if (node != null)
						resource.Created = DateTime.Parse(node.Value, CultureInfo.CurrentCulture);

					node = prop.Element(XName.Get("getlastmodified", "DAV:"));
					if (node != null)
						resource.Modified = DateTime.Parse(node.Value, CultureInfo.CurrentCulture);
					
                    // Check if the resource is a collection
					node = prop.Element(XName.Get("resourcetype", "DAV:")).Element(XName.Get("collection", "DAV:"));
                    resource.IsDirectory = node != null;
					
					node = prop.Element(XName.Get("getcontenttype", "DAV:"));
					if (node != null)
						resource.ContentType = node.Value;

					node = prop.Element(XName.Get("getetag", "DAV:"));
					if (node != null)
						resource.Etag = node.Value;

					node = prop.Element(XName.Get("quota-used-bytes", "DAV:"));
					if (node != null)
						resource.QuotaUsed = long.Parse(node.Value);

					node = prop.Element(XName.Get("quota-available-bytes", "DAV:"));
					if (node != null)
						resource.QutoaAvailable = long.Parse(node.Value);

					if (resource.Name == null) {
						if (resource.IsDirectory)
							resource.Name = resource.Uri.Segments.Last().TrimEnd(new char[]{'/'});
						else
							resource.Name = resource.Uri.Segments.Last();
						
						if (resource.Uri.AbsolutePath.Equals(rootpath))
							resource.Name = "/";
					}

                    webDavResources.Add(resource);
                }
            }
            catch (Exception e)
            {
                // TODO: Implement better error handling
                Debug.WriteLine(e.Message);

                webDavResources = new List<WebDavResource>();
            }

            return webDavResources;
        }
        #endregion
    }
}
