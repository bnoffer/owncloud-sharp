// --------------------------------
// <copyright file="AuthType.cs" company="Thomas Loehlein">
//     WebDavNet - A WebDAV client
//     Copyright (C) 2009 - Thomas Loehlein
//     This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License along with this program; if not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Thomas Loehlein</author>
// <email>thomas.loehlein@gmail.com</email>
// ---------------------------------

namespace WebDav
{
    /// <summary>
    /// All supported web authentication types.
    /// </summary>
	public enum AuthType
	{
        /// <summary>
        /// Basic authentication
        /// </summary>
		Basic,
        /// <summary>
        /// NTLM authentication.
        /// </summary>
		Ntlm,
        /// <summary>
        /// Digest authentication.
        /// </summary>
		Digest,
        /// <summary>
        /// Kerberos authentication.
        /// </summary>
		Kerberos,
        /// <summary>
        /// Negotiate the authentication between Client and Server.
        /// </summary>
		Negotiate
	}
}
