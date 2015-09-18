// --------------------------------
// <copyright file="WebDavResource.cs" company="Thomas Loehlein">
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

namespace WebDav
{
    /// <summary>
    /// Description of WebDavResource.
    /// </summary>
	public class WebDavResource
	{
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
		public string Name
		{ get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
		public int Size
		{ get; set; }

        /// <summary>
        /// Gets or sets the Uri.
        /// </summary>
        /// <value>The Uri.</value>
        public Uri Uri
        { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>The created.</value>
		public DateTime Created
		{ get; set; }

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        /// <value>The modified.</value>
		public DateTime Modified
		{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this resource is a directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this resource is a directory; otherwise, <c>false</c>.
        /// </value>
		public bool IsDirectory
		{ get; set; }
	}
}
