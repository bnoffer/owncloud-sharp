// --------------------------------
// <copyright file="WebDavCredential.cs" company="Thomas Loehlein">
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
using System.Net;

namespace WebDav
{
    /// <summary>
    /// WebDavCredential class is an extension of the NetworkCredential class to support Web authentication.
    /// </summary>
	public class WebDavCredential : NetworkCredential
	{
		#region CONSTRUCTORS
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavCredential"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
		public WebDavCredential(string user, string password)
			: this(user, password, String.Empty)
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavCredential"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="authType">Type of the authentication.</param>
		public WebDavCredential(string user, string password, AuthType authType)
			: this(user, password, String.Empty, authType)
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavCredential"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
		public WebDavCredential(string user, string password, string domain)
			: this(user, password, domain, AuthType.Basic)
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavCredential"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="authType">Type of the authentication.</param>
		public WebDavCredential(string user, string password, string domain, AuthType authType) 
			: base(user, password, domain)
		{
			AuthenticationType = authType;
		}
		#endregion
		
		#region PUBLIC PROPERTIES
        /// <summary>
        /// Gets or sets the type of the authentication.
        /// </summary>
        /// <value>The type of the authentication.</value>
		public AuthType AuthenticationType
		{ get;set; }
		#endregion
	}
}
