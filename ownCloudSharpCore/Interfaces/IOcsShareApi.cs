using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using owncloudsharp.Schemas;

namespace owncloudsharp.Interfaces
{
    [Headers("OCS-APIREQUEST: true")]
    public interface IOcsShareApi
    {
        #region Local Shares

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetAllShares([Header("Authorization")] string authorization,
                                                               string path,
                                                               bool? reshares,
                                                               string shared_with_me,
                                                               string state,
                                                               bool? subfiles);

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetShare([Header("Authorization")] string authorization,
                                                           [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> AcceptShare([Header("Authorization")] string authorization,
                                                              [AliasAs("{shareId}")] string shareId);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeclineShare([Header("Authorization")] string authorization,
                                                               [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> CreateShare([Header("Authorization")] string authorization,
                                                              [Body(BodySerializationMethod.Serialized)] OcsShareRequest body);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeleteShare([Header("Authorization")] string authorization,
                                                              [AliasAs("{shareId}")] string shareId);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> UpdateShare([Header("Authorization")] string authorization,
                                                              [AliasAs("{shareId}")] string shareId,
                                                              [Body(BodySerializationMethod.Serialized)] OcsShareRequest body);

        #endregion

        #region Federated Cloud Shares

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetAllRemoteShares([Header("Authorization")] string authorization);

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetRemoteShare([Header("Authorization")] string authorization,
                                                                 [AliasAs("{shareId}")] string shareId);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeleteRemoteShare([Header("Authorization")] string authorization,
                                                                    [AliasAs("{shareId}")] string shareId);

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares/pending?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetPendingRemoteShares([Header("Authorization")] string authorization);

        [Post("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares/pending/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> AcceptRemoteShare([Header("Authorization")] string authorization,
                                                                    [AliasAs("{shareId}")] string shareId);

        [Delete("/ocs/v1.php/apps/files_sharing/api/v1/remote_shares/pending/{shareId}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeclineRemoteShare([Header("Authorization")] string authorization,
                                                                     [AliasAs("{shareId}")] string shareId);

        #endregion

        #region OCS Recipient API

        [Get("/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json")]
        Task<ApiResponse<OcsRecipientResponseScheme>> GetShareRecipients([Header("Authorization")] string authorization,
                                                                         string search,
                                                                         string itemType,
                                                                         int? shareType,
                                                                         int? page,
                                                                         int? perPage);

        #endregion
    }
}
