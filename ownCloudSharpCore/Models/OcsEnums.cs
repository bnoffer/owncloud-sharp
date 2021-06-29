using System;

namespace owncloudsharp.Models
{
    /// <summary>
    /// Share permissions. Add values to a int in order to set multiple permissions.
    /// See <c>https://doc.owncloud.org/server/8.2/developer_manual/core/ocs-share-api.html</c> for reference.
    /// </summary>
    public enum OcsPermission {
        /// <summary>
        /// Read permission.
        /// </summary>
        Read = 1,
        /// <summary>
        /// Update  permission.
        /// </summary>
        Update = 2,
        /// <summary>
        /// Create  permission.
        /// </summary>
        Create = 4,
        /// <summary>
        /// Delete  permission.
        /// </summary>
        Delete = 8,
        /// <summary>
        /// Share  permission.
        /// </summary>
        Share = 16,
        /// <summary>
        /// All permissions.
        /// </summary>
        All = 31,
        /// <summary>
        /// Not defined indicator.
        /// </summary>
        None = -1
    }

    /// <summary>
    /// Available share types.
    /// See <c>https://doc.owncloud.org/server/8.2/developer_manual/core/ocs-share-api.html</c> for reference.
    /// </summary>
    public enum OcsShareType {
        /// <summary>
        /// User Share.
        /// </summary>
        User = 0,
        /// <summary>
        /// Group Share.
        /// </summary>
        Group = 1,
        /// <summary>
        /// Link Share.
        /// </summary>
        Link = 3,
        /// <summary>
        /// Remote Share.
        /// </summary>
        Remote = 6,
        /// <summary>
        /// Not defined indicator.
        /// </summary>
        None = -1
    }

    /// <summary>
    /// Boolean parameter.
    /// </summary>
    public enum OcsBoolParam {
        /// <summary>
        /// Boolean False.
        /// </summary>
        False = 0,
        /// <summary>
        /// Boolean True.
        /// </summary>
        True = 1,
        /// <summary>
        /// Not defined indicator.
        /// </summary>
        None = -1
    }

    /// <summary>
    /// Defines the key value for SetUserAttribute
    /// </summary>
    public enum OCSUserAttributeKey
    {
        /// <summary>
        /// Users display name
        /// </summary>
        DisplayName = 0,
        /// <summary>
        /// Users storage quota
        /// </summary>
        Quota = 1,
        /// <summary>
        /// Users password
        /// </summary>
        Password = 2,
        /// <summary>
        /// Users e-mail address
        /// </summary>
        EMail = 3
    }
}

