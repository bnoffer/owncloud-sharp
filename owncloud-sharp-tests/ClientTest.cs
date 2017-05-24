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
			if (!c.UserExists("sharetest"))
				c.CreateUser ("sharetest", "test");
			if (!c.GroupExists("testgroup"))
				c.CreateGroup ("testgroup");
			if (!c.IsUserInGroup("sharetest", "testgroup"))
				c.AddUserToGroup ("sharetest", "testgroup");
		}

		/// <summary>
		/// Cleanup test data.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			#region DAV Test CleanUp
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
			#endregion

			#region OCS Share Test CleanUp
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
			#endregion

			#region OCS User Test cleanup
			if (c.UserExists("octestusr1")) {
				var c1 = new Client(TestSettings.ownCloudInstanceUrl, "octestusr1", "test");
				var shares = c1.GetShares("");
				foreach (var share in shares)
					c1.DeleteShare(share.ShareId);
				c.DeleteUser("octestusr1");
			}
			if (c.UserExists("octestusr")) {
				var c2 = new Client(TestSettings.ownCloudInstanceUrl, "octestusr", "test");
				var shares = c2.GetShares("");
				foreach (var share in shares)
					c2.DeleteShare(share.ShareId);
				c.DeleteUser("octestusr");
			}
			#endregion

			#region OCS App Attribute Test Cleanup
			if (c.GetAttribute("files", "test").Count > 0)
				c.DeleteAttribute ("files", "test");
			#endregion

			#region General CleanUp
			var c3 = new Client(TestSettings.ownCloudInstanceUrl, "sharetest", "test");
			var c3shares = c3.GetShares("");
			foreach (var share in c3shares)
				c3.DeleteShare(share.ShareId);
			c.RemoveUserFromGroup ("sharetest", "testgroup");
			c.DeleteGroup ("testgroup");
			c.DeleteUser ("sharetest");
			#endregion
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
		/// <summary>
		/// Test ShareWithLink;
		/// </summary>
		[Test ()]
		public void ShareWithLink() {
			MemoryStream payload = new MemoryStream (payloadData);

			c.Upload ("/share-link-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-link-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			Assert.NotNull (share);
		}

		/// <summary>
		/// Test ShareWithUser.
		/// </summary>
		[Test ()]
		public void ShareWithUser() {
			MemoryStream payload = new MemoryStream (payloadData);

			c.Upload ("/share-user-test.txt", payload, "text/plain");
			var share = c.ShareWithUser ("/share-user-test.txt", "sharetest", Convert.ToInt32 (OcsPermission.All), OcsBoolParam.False);

			Assert.NotNull (share);
		}

		/// <summary>
		/// Test ShareWithGroup.
		/// </summary>
		[Test ()]
		public void ShareWithGroup() {
			MemoryStream payload = new MemoryStream (payloadData);

			c.Upload ("/share-group-test.txt", payload, "text/plain");
			var share = c.ShareWithGroup ("/share-group-test.txt", "testgroup", Convert.ToInt32 (OcsPermission.All));

			Assert.NotNull (share);
		}

		/// <summary>
		/// Test UpdateShare.
		/// </summary>
		[Test ()]
		public void UpdateShare() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-update-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-update-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.UpdateShare (share.ShareId, -1, "test123test");
			Assert.True (result);
		}

		/// <summary>
		/// Test DeleteShare.
		/// </summary>
		[Test ()]
		public void DeleteShare() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-delete-test.txt", payload, "text/plain");
			var share = c.ShareWithLink ("/share-delete-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.DeleteShare (share.ShareId);
			Assert.True (result);
		}

		/// <summary>
		/// Test IsShared.
		/// </summary>
		/// <returns><c>true</c> if this instance is shared; otherwise, <c>false</c>.</returns>
		[Test ()]
		public void IsShared() {
			MemoryStream payload = new MemoryStream (payloadData);

			var result = c.Upload ("/share-shared-test.txt", payload, "text/plain");
			c.ShareWithLink ("/share-shared-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			result = c.IsShared ("/share-shared-test.txt");
			Assert.True (result);
		}

		/// <summary>
		/// Test GetShares for a given path.
		/// </summary>
		[Test ()]
		public void GetSharesForPath() {
			MemoryStream payload = new MemoryStream (payloadData);

			c.Upload ("/share-get-test.txt", payload, "text/plain");
			c.ShareWithLink ("/share-get-test.txt", Convert.ToInt32 (OcsPermission.All), "test", OcsBoolParam.True);

			var content = c.GetShares ("/share-get-test.txt");
			Assert.Greater (content.Count, 0);
		}

		/// <summary>
		/// Test GetShares for the current user.
		/// </summary>
		[Test ()]
		public void GetSharesForUser() {
			var content = c.GetShares ("");
			Assert.Greater (content.Count, 0);
		}
		#endregion

		#region Users
		/// <summary>
		/// Test CreateUser.
		/// </summary>
		[Test ()]
		public void CreateUser() {
			var result = c.CreateUser ("octestusr1", "octestpwd");
			Assert.True (result);
		}

		/// <summary>
		/// Test DeleteUser.
		/// </summary>
		[Test ()]
		public void DeleteUser() {
			var result = c.CreateUser ("deluser", "delpwd");
			if (result)
				result = c.DeleteUser ("deluser");
			Assert.True (result);
		}

		/// <summary>
		/// Test UserExists.
		/// </summary>
		[Test ()]
		public void UserExists() {
			var result = c.UserExists ("sharetest");
			Assert.True (result);
		}

		/// <summary>
		/// Test SearchUsers.
		/// </summary>
		[Test ()]
		public void SearchUsers() {
			var result = c.SearchUsers ("sharetest");
			Assert.Greater (result.Count, 0);
		}

		/// <summary>
		/// Test GetUserAttributes.
		/// </summary>
		[Test ()]
		public void GetUserAttributes() {
			var result = c.GetUserAttributes ("sharetest");
			Assert.NotNull (result);
		}

		/// <summary>
		/// Test SetUserAttribute.
		/// </summary>
		[Test ()]
		public void SetUserAttribute() {
			var result = c.SetUserAttribute ("sharetest", OCSUserAttributeKey.EMail, "demo@example.com");
			Assert.True (result);
		}

		/// <summary>
		/// Test AddUserToGroup.
		/// </summary>
		[Test ()]
		public void AddUserToGroup() {
			if (!c.UserExists("octestusr"))
				c.CreateUser ("octestusr", "octestpwd");

			var result = c.AddUserToGroup ("octestusr", "testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test GetUserGroups.
		/// </summary>
		[Test ()]
		public void GetUserGroups() {
			var result = c.GetUserGroups ("octestusr");
			Assert.GreaterOrEqual (result.Count, 0);
		}

		/// <summary>
		/// Test IsUserInGroup.
		/// </summary>
		/// <returns><c>true</c> if this instance is user in group; otherwise, <c>false</c>.</returns>
		[Test ()]
		public void IsUserInGroup() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}

			var result = c.IsUserInGroup ("octestusr", "testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test IsUserNotInGroup.
		/// </summary>
		/// <returns><c>true</c> if this instance is user not in group; otherwise, <c>false</c>.</returns>
		[Test ()]
		public void IsUserNotInGroup() {
			var result = c.IsUserInGroup (TestSettings.ownCloudUser, "testgroup");
			Assert.False (result);
		}

		/// <summary>
		/// Test RemoveUserFromGroup.
		/// </summary>
		[Test ()]
		public void RemoveUserFromGroup() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}
			if (!c.IsUserInGroup("octestusr", "testgroup"))
				c.AddUserToGroup ("octestusr", "testgroup");

			var result = c.RemoveUserFromGroup("octestusr", "testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test AddUserToSubAdminGroup.
		/// </summary>
		[Test ()]
		public void AddUserToSubAdminGroup() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}

			var result = c.AddUserToSubAdminGroup("octestusr", "testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test GetUserSubAdminGroups.
		/// </summary>
		[Test ()]
		public void GetUserSubAdminGroups() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}
			if (!c.IsUserInSubAdminGroup("octestusr", "testgroup"))
				c.AddUserToSubAdminGroup("octestusr", "testgroup");

			var result = c.GetUserSubAdminGroups ("octestusr");
			Assert.NotNull (result);
		}

		/// <summary>
		/// Test IsUserInSubAdminGroup.
		/// </summary>
		/// <returns><c>true</c> if this instance is user in sub admin group; otherwise, <c>false</c>.</returns>
		[Test ()]
		public void IsUserInSubAdminGroup() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}
			if (!c.IsUserInSubAdminGroup("octestusr", "testgroup"))
				c.AddUserToSubAdminGroup("octestusr", "testgroup");

			var result = c.IsUserInSubAdminGroup("octestusr", "testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test RemoveUserFromSubAdminGroup.
		/// </summary>
		public void RemoveUserFromSubAdminGroup() {
			if (!c.UserExists ("octestusr")) {
				c.CreateUser ("octestusr", "octestpwd");
				c.AddUserToGroup ("octestusr", "testgroup");
			}
			if (!c.IsUserInSubAdminGroup("octestusr", "testgroup"))
				c.AddUserToSubAdminGroup("octestusr", "testgroup");

			var result = c.RemoveUserFromSubAdminGroup("octestusr", "testgroup");
			Assert.True (result);
		}
		#endregion

		#region Groups
		/// <summary>
		/// Test CreateGroup.
		/// </summary>
		[Test ()]
		public void CreateGroup() {
			var result = c.CreateGroup ("ocsgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test DeleteGroup.
		/// </summary>
		[Test ()]
		public void DeleteGroup() {
			if (!c.GroupExists("ocsgroup"))
				c.CreateGroup ("ocsgroup");
			var result = c.DeleteGroup ("ocsgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test GroupExists with existing group.
		/// </summary>
		[Test ()]
		public void GroupExists() {
			var result = c.GroupExists ("testgroup");
			Assert.True (result);
		}

		/// <summary>
		/// Test GroupExists with not existing group.
		/// </summary>
		public void GroupNotExists() {
			var result = c.GroupExists ("ocs-does-not-exist");
			Assert.False (result);
		}

        /// <summary>
		/// Test SearchGroups.
		/// </summary>
		[Test()]
        public void SearchGroups()
        {
            var result = c.SearchGroups("testgroup");
            Assert.Greater(result.Count, 0);
        }
        #endregion

        #region Config
        /// <summary>
        /// Test GetConfig.
        /// </summary>
        [Test ()]
		public void GetConfig() {
			var result = c.GetConfig ();
			Assert.NotNull (result);
		}
		#endregion

		#region Application attributes
		/// <summary>
		/// Test GetAttribute.
		/// </summary>
		public void GetAttribute() {
			var result = c.GetAttribute ("files");
			Assert.NotNull (result);
		}

		/// <summary>
		/// Test SetAttribute.
		/// </summary>
		public void SetAttribute() {
			var result = c.SetAttribute ("files", "test", "true");
			Assert.True (result);
		}

		/// <summary>
		/// Test DeleteAttribute.
		/// </summary>
		public void DeleteAttribute() {
			if (c.GetAttribute("files", "test").Count == 0)
				c.SetAttribute ("files", "test", "true");

			var result = c.DeleteAttribute ("files", "test");
			Assert.True (result);
		}
		#endregion

		#region Apps
		/// <summary>
		/// Test GetApps.
		/// </summary>
		[Test ()]
		public void GetApps() {
			var result = c.GetApps ();
			Assert.Greater (result.Count, 0);
		}

		/// <summary>
		/// Test GetApp.
		/// </summary>
		[Test ()]
		public void GetApp() {
			var result = c.GetApp ("files");
			Assert.NotNull (result);
			Assert.IsNotEmpty (result.Id);
		}

		/// <summary>
		/// Test EnableApp.
		/// </summary>
		[Test ()]
		public void EnableApp() {
			var result = c.EnableApp ("news");
			Assert.True (result);
		}

		/// <summary>
		/// Test DisableApp.
		/// </summary>
		[Test ()]
		public void DisableApp() {
			var result = c.DisableApp ("news");
			Assert.True (result);
		}
		#endregion
		#endregion
	}
}

