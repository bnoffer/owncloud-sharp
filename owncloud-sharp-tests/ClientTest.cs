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
			c.CreateUser ("sharetest", "test");
			c.CreateGroup ("testgroup");
			c.AddUserToGroup ("sharetest", "testgroup");
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

			if (c.Exists ("/share-link-test.txt")) {
				if (c.IsShared ("/share-link-test.txt")) {
					var shares = c.GetShares ("/share-link-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-link-test.txt");
			}

			if (c.Exists ("/share-user-test.txt")) {
				if (c.IsShared ("/share-user-test.txt")) {
					var shares = c.GetShares ("/share-user-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-user-test.txt");
			}

			if (c.Exists ("/share-group-test.txt")) {
				if (c.IsShared ("/share-group-test.txt")) {
					var shares = c.GetShares ("/share-group-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-group-test.txt");
			}

			if (c.Exists ("/share-update-test.txt")) {
				if (c.IsShared ("/share-update-test.txt")) {
					var shares = c.GetShares ("/share-update-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-update-test.txt");
			}

			if (c.Exists ("/share-delete-test.txt")) {
				if (c.IsShared ("/share-delete-test.txt")) {
					var shares = c.GetShares ("/share-delete-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-delete-test.txt");
			}

			if (c.Exists ("/share-shared-test.txt")) {
				if (c.IsShared ("/share-shared-test.txt")) {
					var shares = c.GetShares ("/share-shared-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-shared-test.txt");
			}

			if (c.Exists ("/share-get-test.txt")) {
				if (c.IsShared ("/share-get-test.txt")) {
					var shares = c.GetShares ("/share-get-test.txt");
					foreach (var share in shares)
						c.DeleteShare (share.ShareId);
				}
				c.Delete ("/share-get-test.txt");
			}

			c.RemoveUserFromGroup ("sharetest", "testgroup");
			c.DeleteGroup ("testgroup");
			c.DeleteUser ("sharetest");
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

		#region OCS Tests
		#region Remote Shares
		/*
		 * Deactivated because of testability limitations.
		 * OC 8.2 is not officialy released and currently I have only one OC 8.2 dev instance running.
		[Test ()]
		public void ListOpenRemoteShare() {
			// TODO: Implement ListOpenRemoteShare Test
		}

		[Test ()]
		public void AcceptRemoteShare() {
			// TODO: Implement AcceptRemoteShare Test
		}

		[Test ()]
		public void DeclineRemoteShare() {
			// TODO: Implement AcceptRemoteShare Test
		}

		[Test ()]
		public void ShareWithRemote() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-remote-test.txt", payload, "text/plain");
			var share = c.ShareWithUser ("/share-remote-test.txt", "user@example.com", Convert.ToInt32 (OcsPermission.All), OcsBoolParam.True);

			Assert.NotNull (share);
		}*/
		#endregion

		#region Shares
		[Test ()]
		public void ShareWithLink() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-link-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-link-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			Assert.NotNull (share);
		}

		[Test ()]
		public void ShareWithUser() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-user-test.txt", payload, "text/plain");
			var share = c.ShareWithUser ("/share-user-test.txt", "sharetest", Convert.ToInt32 (OcsPermission.All), OcsBoolParam.False);

			Assert.NotNull (share);
		}

		[Test ()]
		public void ShareWithGroup() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-group-test.txt", payload, "text/plain");
			var share = c.ShareWithGroup ("/share-group-test.txt", "testgroup", Convert.ToInt32 (OcsPermission.All));

			Assert.NotNull (share);
		}

		[Test ()]
		public void UpdateShare() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-update-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-update-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.UpdateShare (share.ShareId, -1, "test123test");
			Assert.True (result);
		}

		[Test ()]
		public void DeleteShare() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-delete-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-delete-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.DeleteShare (share.ShareId);
			Assert.True (result);
		}

		[Test ()]
		public void IsShare() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-shared-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-shared-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.IsShared ("/share-shared-test.txt");
			Assert.True (result);
		}

		[Test ()]
		public void GetShares() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-get-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-get-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			var content = c.GetShares ("/share-get-test.txt");
			Assert.Greater (content.Count, 0);
		}
		#endregion
		#endregion
	}
}

