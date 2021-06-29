using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Polly;
using Refit;
using owncloudsharp.Abstractions;
using owncloudsharp.Interfaces;
using owncloudsharp.Schemas;

namespace owncloudsharp.Components
{
    public class OcsProvisioningApiComponent : BaseComponent
    {
        private string _authHeader;
        private string _baseUrl;

        /// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Components.OcsProvisioningApiComponent"/> class.
		/// </summary>
		/// <param name="url">ownCloud instance URL.</param>
		/// <param name="user_id">User identifier.</param>
		/// <param name="password">Password.</param>
        public OcsProvisioningApiComponent(string url, string user_id, string password)
        {
            _authHeader = FormatBasicAuthHeader(user_id, password);
            _baseUrl = url;
        }
    }
}
