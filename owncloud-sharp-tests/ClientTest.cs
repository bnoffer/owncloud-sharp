using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

using owncloudsharp;

namespace owncloudsharp.Tests
{
	/// <summary>
	/// Tests the ownCloud# client.
	/// 
	/// NOTE: A ownCloud 8.2 installation is required for successful testing.
	/// NOTE: Before testing setup the ownCloud Server URL, Username and Password in TestSettings.cs
	/// </summary>
	[TestFixture ()]
	public class ClientTest
	{
		#region Members
		/// <summary>
		/// ownCloud# instance.
		/// </summary>
		private Client c;
		/// <summary>
		/// File upload payload data.
		/// </summary>
		private byte[] payloadData;
		#endregion

		#region Setup and Tear Down
		/// <summary>
		/// Init this test parameters.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			c = new Client (TestSettings.ownCloudInstanceUrl, TestSettings.ownCloudUser, TestSettings.ownCloudPassword);
			payloadData = System.Text.Encoding.UTF8.GetBytes ("owncloud# NUnit Payload\r\nPlease feel free to delete");
		}

		/// <summary>
		/// Cleanup test data.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			if (c.Exists(TestSettings.testFileName))
				c.Delete (TestSettings.testFileName);
			if (c.Exists(TestSettings.testDirName))
				c.Delete (TestSettings.testDirName);
			if (c.Exists ("/copy-test")) {
				c.Delete ("/copy-test/file.txt");
				c.Delete ("/copy-test");
			}
			if (c.Exists ("/move-test")) {
				c.Delete ("/move-test/file.txt");
				c.Delete ("/move-test");
			}

			if (c.Exists ("/zip-test")) {
				c.Delete ("/zip-test/file.txt");
				c.Delete ("/zip-test");
			}
		}
		#endregion

		#region DAV Tests
		/// <summary>
		/// Test the file upload.
		/// </summary>
		[Test ()]
		public void Upload ()
		{
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");
			Assert.True (result);
		}

		/// <summary>
		/// Tests if a file exists.
		/// </summary>
		[Test ()]
		public void Exists() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if (result)
				result = c.Exists (TestSettings.testFileName);
			
			Assert.True (result);
		}

		/// <summary>
		/// Tests if the file does not exist.
		/// </summary>
		[Test ()]
		public void NotExists() {
			var result = c.Exists ("/this-does-not-exist.txt");
			Assert.False (result);
		}

		/// <summary>
		/// Tests file download.
		/// </summary>
		[Test ()]
		public void Download() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if (result) {
				var content = c.Download (TestSettings.testFileName);
				result = (content != null) ? true : false;
			}

			Assert.True (result);
		}

		/// <summary>
		/// Tests file deletion.
		/// </summary>
		[Test ()]
		public void Delete() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if (result)
				result = c.Delete (TestSettings.testFileName);

			Assert.True (result);
		}

		/// <summary>
		/// Tests directory creation.
		/// </summary>
		[Test ()]
		public void CreateDirectory() {
			var result = c.CreateDirectory (TestSettings.testDirName);
			Assert.True (result);
		}

		/// <summary>
		/// Tests directory deletion.
		/// </summary>
		[Test ()]
		public void DeleteDirectory() {
			var result = true;

			if (!c.Exists(TestSettings.testDirName))
				result = c.CreateDirectory (TestSettings.testDirName);

			if (result)
				result = c.Delete (TestSettings.testDirName);

			Assert.True (result);
		}

		/// <summary>
		/// Tests list command.
		/// </summary>
		[Test ()]
		public void List() {
			var result = c.List ("/");
			Assert.Greater (result.Count, 0);
		}

		/// <summary>
		/// Tests getting resource information.
		/// </summary>
		[Test ()]
		public void GetResourceInfo() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if (result) {
				var content = c.GetResourceInfo (TestSettings.testFileName);
				result = (content != null) ? true : false;
			}

			Assert.True (result);
		}

		/// <summary>
		/// Tests copying files.
		/// </summary>
		[Test ()]
		public void Copy() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if ((result) && (!c.Exists("/copy-test")))
				result = c.CreateDirectory ("/copy-test");

			if (result)
				result = c.Copy (TestSettings.testFileName, "/copy-test/file.txt");

			Assert.True (result);
		}

		/// <summary>
		/// Tests moving files.
		/// </summary>
		[Test ()]
		public void Move() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if ((result) && (!c.Exists("/move-test")))
				result = c.CreateDirectory ("/move-test");

			if (result)
				result = c.Move (TestSettings.testFileName, "/move-test/file.txt");

			Assert.True (result);
		}

		/// <summary>
		/// Tests downloading a direcotry as ZIP file.
		/// </summary>
		[Test ()]
		public void DownloadDirectoryAsZip() {
			MemoryStream payload = new MemoryStream (payloadData);
			var result = c.Upload (TestSettings.testFileName, payload, "text/plain");

			if ((result) && (!c.Exists("/zip-test")))
				result = c.CreateDirectory ("/zip-test");

			if (result) {
				var content = c.DownloadDirectoryAsZip ("/zip-test");
				result = (content != null) ? true : false;
			}

			Assert.True (result);
		}
		#endregion
	}
}

