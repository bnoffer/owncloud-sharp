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
    public class OcsShareApiComponent : BaseComponent
    {
        private string _authHeader;
        private string _baseUrl;

        /// <summary>
		/// Initializes a new instance of the <see cref="owncloudsharp.Components.OcsShareApiComponent"/> class.
		/// </summary>
		/// <param name="url">ownCloud instance URL.</param>
		/// <param name="user_id">User identifier.</param>
		/// <param name="password">Password.</param>
        public OcsShareApiComponent(string url, string user_id, string password)
        {
            _authHeader = FormatBasicAuthHeader(user_id, password);
            _baseUrl = url;
        }

        public async void GetAllShares(string path = null, bool? reshares = null, string shared_with_me = null, string state = null, bool? subfiles = null)
        {
            var tag = $"{this}.GetAllShares";
            try
            {
                var response = await AllSharesGET(_authHeader, path, reshares, shared_with_me, state, subfiles);
                if (response.Code == HttpStatusCode.OK &&
                    response.Result != null)
                {

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Track.Exception(tag, ex);
            }
        }

        private async Task<(HttpStatusCode Code, OcsShareResponseSchema Result)> AllSharesGET(string authentication, string path = null, bool? reshares = null, string shared_with_me = null, string state = null, bool? subfiles = null)
        {
            var tag = $"{this}.AllSharesGET";
            try
            {
                var apiResponse = RestService.For<IOcsShareApi>(_baseUrl);

                PolicyResult<ApiResponse<OcsShareResponseSchema>> pollyResult = null;

                pollyResult = await Policy.ExecuteAndCaptureAsync(async () => await apiResponse.GetAllShares(authentication, path, reshares, shared_with_me, state, subfiles));

                if (pollyResult.Result.Content != null)
                {
                    var result = pollyResult.Result;

                    return (result.StatusCode, result.Content);
                }

                return (pollyResult.Result.StatusCode, null);
            }
            catch (UriFormatException ex)
            {
                Track.Exception(tag, ex);

                return (HttpStatusCode.BadRequest, null);
            }
            catch (ArgumentException ex)
            {
                Track.Exception(tag, ex);

                return (HttpStatusCode.BadGateway, null);
            }
            catch (Exception ex)
            {
                Track.Exception(tag, ex);

                return (HttpStatusCode.InternalServerError, null);
            }
        }
    }
}
