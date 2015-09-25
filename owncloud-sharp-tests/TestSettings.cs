using System;

namespace owncloudsharp.Tests
{
	/// <summary>
	/// Defines settings used in the ClientTest Fixture.
	/// </summary>
	public class TestSettings
	{
		// TODO: CONFIGURE BEFORE RUNNING TESTS
		/// <summary>
		/// The ownCloud instance URL.
		/// </summary>
		public const string ownCloudInstanceUrl = "http://192.168.2.245/owncloud-devel";
		/// <summary>
		/// The ownCloud user.
		/// </summary>
		public const string ownCloudUser = "admin";
		/// <summary>
		/// The ownCloud password.
		/// </summary>
		public const string ownCloudPassword = "Bno131083";
		public const string testFileName = "/owncloud-sharp-test.txt";
		public const string testDirName = "/owncloud-sharp-test-folder";
	}
}

