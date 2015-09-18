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
using System.Net;
using System.Reflection;
using System.Threading;
//using System.Windows.Forms;
using System.Xml;

namespace WebDav
{
    /// <summary>
    /// Base manager class for handling all WebDav purpose.
    /// </summary>
    public class WebDavManager
    {
        #region EVENTS
        /// <summary>
        /// Event that fires when a download proceed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressEvent;
        /// <summary>
        /// Event that fires when an upload proceed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> UploadProgressEvent;
        #endregion

        #region PRIVATE PROPERTIES
        private readonly ThreadSafeDictionary<Uri, string> _listDownloads = new ThreadSafeDictionary<Uri, string>();
        private readonly ThreadSafeDictionary<Uri, string> _listUploads = new ThreadSafeDictionary<Uri, string>();
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavManager"/> class.
        /// </summary>
        public WebDavManager()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavManager"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public WebDavManager(WebDavCredential credential)
        {
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
        	HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.Delete);
            HttpWebResponse webResponse;


        	try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + uri);

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
        	HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.MkCol);
            HttpWebResponse webResponse;

			try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + uri);

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
        	HttpWebRequest webRequest = GetBaseRequest(sourceUri, RequestMethod.Copy);
            HttpWebResponse webResponse;

            webRequest.Headers.Add("Destination", targetUri.ToString());
        	
        	// TODO: Handle overwrite
        	//_WebRequest.Headers.Add("Overwrite", "F") // No Overwrite
        	
            try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + sourceUri);

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
        	HttpWebRequest webRequest = GetBaseRequest(sourceUri, RequestMethod.Move);
            HttpWebResponse webResponse;

        	webRequest.Headers.Add("Destination", targetUri.ToString());
        	
        	// TODO: Handle overwrite take a look at http://www.ietf.org/rfc/rfc2518.txt first
        	//_WebRequest.Headers.Add("Overwrite", "F") // No Overwrite
        	//_WebRequest.Headers.Add("Overwrite", "T") // ???
        	
			try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + sourceUri);

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
        	HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.Head);
            HttpWebResponse webResponse;

			try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + uri);

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
        	
        	HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.PropFind);
            HttpWebResponse webResponse;
        	
        	// Retrieve only the requested folder
        	webRequest.Headers.Add("Depth", "1");
        	
			try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();

                listResource = ExtractResources(webResponse.GetResponseStream());
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
                Debug.WriteLine("Empty WebResponse @'" + MethodBase.GetCurrentMethod().Name + "'" + Environment.NewLine + uri);

                return new List<WebDavResource>();
            }
            
            return listResource;
        }
        
        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <param name="localFile">The local file.</param>
        public int UploadFile(string url, string localFile)
        {
            return UploadFile(new Uri(url), localFile);
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <param name="localFile">The local file.</param>
        public int UploadFile(Uri uri, string localFile)
        {
            HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.Put);
            HttpWebResponse webResponse;

            Stream remoteStream = null;
            FileStream localStream = null;

            byte[] buffer = new byte[1024];
            int bytesRead;
            int totalSent = 0;
            
            KeyValuePair<Uri, string> keyValuePair = new KeyValuePair<Uri, string>(uri, localFile);
            _listUploads.Add(keyValuePair);

            try
            {
                // Read the local file
                FileInfo fileInfo = new FileInfo(localFile);
                localStream = fileInfo.OpenRead();

                // Disable write buffer to avoid OutOfMemory situations with large files
                webRequest.AllowWriteStreamBuffering = false;

                // Have to set the ContentLength when AllowWriteStreamBuffering is disabled
                webRequest.ContentLength = localStream.Length;

                // Get the request stream
                remoteStream = webRequest.GetRequestStream();
                
                bool bRun;

                // Loop through stream until no bytes are there anymore
                do
                {
                	bRun = _listUploads.Contains(keyValuePair);
                	
                    // Read into the buffer from the remote stream
                    bytesRead = localStream.Read(buffer, 0, buffer.Length);

                    // Write into the file stream
                    remoteStream.Write(buffer, 0, bytesRead);
                    
                    totalSent += bytesRead;
                    
                    if (UploadProgressEvent != null)
                    	InvokeProgressEvent(UploadProgressEvent, new ProgressEventArgs(bytesRead <= 0, fileInfo.FullName, uri, totalSent, fileInfo.Length));
                    
                } while (bytesRead > 0 && bRun);

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                
                // TODO: Extract possible status values from the response
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);
                
                totalSent = -1;
            }
            finally
            {
                if (remoteStream != null)
                {
                    remoteStream.Close();
                    remoteStream.Dispose();
                }

                if (localStream != null)
                {
                    localStream.Close();
                    localStream.Dispose();
                }

                // Clear item of the upload list
                if (_listUploads.Contains(keyValuePair))
                	_listUploads.Remove(keyValuePair);
            }
            
            return totalSent;
        }
        
        /// <summary>
        /// Uploads the file asynchronous.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <param name="localFile">The local file.</param>
        public UploadAsyncHandler UploadFileAsync(Uri uri, string localFile)
        {
        	UploadAsyncHandler handler = new UploadAsyncHandler(this, uri, localFile);
        	
        	ThreadPool.QueueUserWorkItem(delegate { UploadFile(uri, localFile); });
        	
        	return handler;
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="url">The Url.</param>
        /// <param name="localFile">The local file.</param>
        public int DownloadFile(string url, string localFile)
        {
            return DownloadFile(new Uri(url), localFile);
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <param name="localFile">The local file.</param>
        public int DownloadFile(Uri uri, string localFile)
        {
            HttpWebRequest webRequest = GetBaseRequest(uri, RequestMethod.Get);
            HttpWebResponse webResponse;

            try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);

                return -1;
            }

            if (webResponse == null)
            {
                // TODO: Errorhandling
                Debug.WriteLine("Empty WebResponse " + Environment.NewLine + uri + Environment.NewLine + localFile);

                return -1;
            }

            byte[] buffer = new byte[1024];
            int bytesRead;
            int totalRead = 0;

            Stream remoteStream = null;
            Stream localStream = null;

            KeyValuePair<Uri, string> keyValuePair = new KeyValuePair<Uri, string>(uri, localFile);
            _listDownloads.Add(keyValuePair);

            try
            {
                // Get the stream object of the response
                remoteStream = webResponse.GetResponseStream();

                // Create the local file
                FileInfo fileInfo = new FileInfo(localFile);
                localStream = fileInfo.Create();

                bool bRun;

                // Loop through stream until no bytes are there anymore
                do
                {
                    bRun = _listDownloads.Contains(keyValuePair);

                    // Read into the buffer from the remote stream
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    // Write into the file stream
                    localStream.Write(buffer, 0, bytesRead);
                    
                    totalRead += bytesRead;

                    if (DownloadProgressEvent != null)
                        InvokeProgressEvent(DownloadProgressEvent, new ProgressEventArgs(bytesRead <= 0, fileInfo.FullName, webResponse.ResponseUri, totalRead, webResponse.ContentLength));
                } while (bytesRead > 0 && bRun);
            }
            catch (Exception e)
            {
                // TODO: Errorhandling
                Debug.WriteLine(e.Message);
                
                totalRead = -1;
            }
            finally
            {
                // Close and Dispose of all created objects
                if (webResponse != null)
                    webResponse.Close();

                if (remoteStream != null)
                {
                    remoteStream.Close();
                    remoteStream.Dispose();
                }

                if (localStream != null)
                {
                    localStream.Close();
                    localStream.Dispose();
                }

                // Clear item of the download list
                if (_listDownloads.Contains(keyValuePair))
                    _listDownloads.Remove(keyValuePair);
            }
            
            return totalRead;
        }

		/// <summary>
        /// Downloads the file asynchronous.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <param name="localFile">The local file.</param>
        public DownloadAsyncHandler DownloadFileAsync(Uri uri, string localFile)
        {
            DownloadAsyncHandler handler = new DownloadAsyncHandler(this, uri, localFile);

            ThreadPool.QueueUserWorkItem(delegate { DownloadFile(uri, localFile); });

            return handler;
        }
        #endregion

        #region PRIVATE METHODS
        internal void AbortDownload(Uri remoteUri, string localFile)
        {
            KeyValuePair<Uri, string> pair = new KeyValuePair<Uri, string>(remoteUri, localFile);

            if (_listDownloads.Contains(pair))
                _listDownloads.Remove(pair);
        }

        internal void AbortUpload(Uri remoteUri, string localFile)
        {
        	KeyValuePair<Uri, string> pair = new KeyValuePair<Uri, string>(remoteUri, localFile);
        	
        	if (_listUploads.Contains(pair))
        		_listUploads.Remove(pair);
        }

        private void InvokeProgressEvent(EventHandler<ProgressEventArgs> progressEventHandler, ProgressEventArgs args)
        {
            if (DownloadProgressEvent == null)
                return;

            foreach (Delegate handler in progressEventHandler.GetInvocationList())
            {
                /*if (handler.Target is Control)
                {
                    Control target = handler.Target as Control;

                    if (target.IsHandleCreated)
                        target.BeginInvoke(handler, this, args);
                }
                else*/ 
				if (handler.Target is ISynchronizeInvoke)
                {
                    ISynchronizeInvoke target = handler.Target as ISynchronizeInvoke;
                    target.BeginInvoke(handler, new object[] { this, args });
                }
                else
                    handler.DynamicInvoke(this, args);
            } 
        }

        private HttpWebRequest GetBaseRequest(Uri uri, RequestMethod method)
        {
            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri);

            // Set the credentials if available
            if (Credential != null)
            {
                CredentialCache credentialCache = new CredentialCache {{uri, Credential.AuthenticationType.ToString(), Credential}};

                webRequest.Credentials = credentialCache;
            }
            
            // Set the proxy if available
            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }

            // Set default headers
            webRequest.Headers.Set("Pragma", "no-cache");

            // Set the request timeout
            if (Timeout < 1)
                Timeout = 30000; // At least 30 Seconds

            webRequest.Timeout = Timeout;

            // TODO: Check if PreAuthenticate is necessary
            // Set PreAuthenticate to assimilate authentication from the plain HEAD request
            //_WebRequest.PreAuthenticate = true;

            // Set the request method
            webRequest.Method = method.ToString().ToUpper(CultureInfo.InvariantCulture);

            return webRequest;
        }

        private static List<WebDavResource> ExtractResources(Stream strm)
        {
            List<WebDavResource> webDavResources = new List<WebDavResource>();

            try
            {
                XmlReader xread = XmlReader.Create(strm);
                XmlDocument xdoc = new XmlDocument();

                XmlNamespaceManager xman = new XmlNamespaceManager(xread.NameTable);
                xman.AddNamespace("D", "DAV:");

                xdoc.Load(xread);

                XmlNodeList xlist = xdoc.SelectNodes("//D:response", xman);

                for (int i = 0; i < xlist.Count; i++)
                {
                    WebDavResource resource = new WebDavResource();

                    // Do not add hidden files
                    // Hidden files cannot be downloaded from the IIs
                    // For further information see http://support.microsoft.com/kb/216803/

                    XmlNode node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:ishidden", xman);
                    if (node != null && node.InnerText == "1")
                        continue;

                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:displayname", xman);
                    if (node != null)
                        resource.Name = node.InnerText;

                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:href", xman);
                    if (node != null)
                    {
                    	Uri href;

                        if (Uri.TryCreate(node.InnerText, UriKind.Absolute, out href))
                            resource.Uri = href;
                    }

                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:getcontentlength", xman);
                    if (node != null)
                        resource.Size = int.Parse(node.InnerText, CultureInfo.CurrentCulture);

                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:creationdate", xman);
                    if (node != null)
                        resource.Created = DateTime.Parse(node.InnerText, CultureInfo.CurrentCulture);

                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:getlastmodified", xman);
                    if (node != null)
                        resource.Modified = DateTime.Parse(node.InnerText, CultureInfo.CurrentCulture);

                    // Check if the resource is a collection
                    node = xdoc.SelectSingleNode("//D:response[" + (i + 1) + "]/D:propstat/D:prop/D:resourcetype/D:collection", xman);
                    resource.IsDirectory = node != null;

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
