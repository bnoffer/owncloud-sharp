using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using owncloudsharp.Schemas;

namespace owncloudsharp.Interfaces
{
    [Headers("OCS-APIREQUEST: true")]
    public interface IOcsApi
    {
        #region Local Shares

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json")]
        Task<ApiResponse<OcsResponseSchema>> GetAllShares([Header("Authorization")] string authorization,
                                                                 string path,
                                                                 bool? reshares,
                                                                 string shared_with_me,
                                                                 string state,
                                                                 bool? subfiles);

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsResponseSchema>> GetShare([Header("Authorization")] string authorization,
                                                      [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsResponseSchema>> AcceptShare([Header("Authorization")] string authorization,
                                                         [AliasAs("{shareId}")] string shareId);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsResponseSchema>> DeclineShare([Header("Authorization")] string authorization,
                                                          [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json")]
        Task<ApiResponse<OcsResponseSchema>> CreateShare([Header("Authorization")] string authorization,
                                                         [Body(BodySerializationMethod.Serialized)] OcsShareRequest body);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsResponseSchema>> DeleteShare([Header("Authorization")] string authorization,
                                                         [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsResponseSchema>> UpdateShare([Header("Authorization")] string authorization,
                                                         [AliasAs("{shareId}")] string shareId,
                                                         [Body(BodySerializationMethod.Serialized)] OcsShareRequest body);

        #endregion
    }
}
