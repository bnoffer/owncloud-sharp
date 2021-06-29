﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using owncloudsharp.Schemas;

namespace owncloudsharp.Interfaces
{
    public interface IOcsProvisioningApi
    {
        #region Users

        [Post("/ocs/v1.php/cloud/users?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> CreateUser([Header("Authorization")] string authorization,
                                                             [Body(BodySerializationMethod.Serialized)] OcsCreateUserRequest body);

        [Get("/ocs/v1.php/cloud/users?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetUsers([Header("Authorization")] string authorization,
                                                           string search,
                                                           int? limit,
                                                           int? offset);

        [Get("/ocs/v1.php/cloud/users/{userid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetUser([Header("Authorization")] string authorization,
                                                          [AliasAs("userid")] string userid);

        [Put("/ocs/v1.php/cloud/users/{userid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> UpdateUser([Header("Authorization")] string authorization,
                                                             [AliasAs("userid")] string userid,
                                                             [Body(BodySerializationMethod.Serialized)] OcsUpdateUserRequest body);

        [Put("/ocs/v1.php/cloud/users/{userid}/enable?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> EnableUser([Header("Authorization")] string authorization,
                                                             [AliasAs("userid")] string userid);

        [Put("/ocs/v1.php/cloud/users/{userid}/disable?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DisableUser([Header("Authorization")] string authorization,
                                                             [AliasAs("userid")] string userid);

        [Delete("/ocs/v1.php/cloud/users/{userid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeleteUser([Header("Authorization")] string authorization,
                                                             [AliasAs("userid")] string userid);

        #endregion

        #region User Groups

        [Get("/ocs/v1.php/cloud/users/{userid}/groups?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetGroups([Header("Authorization")] string authorization,
                                                            [AliasAs("userid")] string userid);

        [Post("/ocs/v1.php/cloud/users/{userid}/groups?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> AddToGroups([Header("Authorization")] string authorization,
                                                              [AliasAs("userid")] string userid,
                                                              [Body(BodySerializationMethod.Serialized)] OcsGroupRequest body);

        [Delete("/ocs/v1.php/cloud/users/{userid}/groups?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> RemoveFromGroups([Header("Authorization")] string authorization,
                                                                   [AliasAs("userid")] string userid,
                                                                   [Body(BodySerializationMethod.Serialized)] OcsGroupRequest body);

        #endregion

        #region User SubAdmins

        [Post("/ocs/v1.php/cloud/users/{userid}/subadmins?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> CreateSubAdmin([Header("Authorization")] string authorization,
                                                                 [AliasAs("userid")] string userid,
                                                                 [Body(BodySerializationMethod.Serialized)] OcsSubAdminRequest body);

        [Delete("/ocs/v1.php/cloud/users/{userid}/subadmins?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeleteSubAdmin([Header("Authorization")] string authorization,
                                                                 [AliasAs("userid")] string userid,
                                                                 [Body(BodySerializationMethod.Serialized)] OcsSubAdminRequest body);

        [Get("/ocs/v1.php/cloud/users/{userid}/subadmins?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetSubAdmin([Header("Authorization")] string authorization,
                                                              [AliasAs("userid")] string userid);

        #endregion

        #region Groups

        [Get("/ocs/v1.php/cloud/groups?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetGroups([Header("Authorization")] string authorization,
                                                            string search,
                                                            int? limit,
                                                            int? offset);

        [Post("/ocs/v1.php/cloud/groups?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> AddGroup([Header("Authorization")] string authorization,
                                                           [Body(BodySerializationMethod.Serialized)] OcsGroupRequest body);

        [Get("/ocs/v1.php/cloud/groups/{groupid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetGroup([Header("Authorization")] string authorization,
                                                           [AliasAs("groupid")] string groupid);

        [Get("/ocs/v1.php/cloud/groups/{groupid}/subadmins?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetGroupSubAdmins([Header("Authorization")] string authorization,
                                                                    [AliasAs("groupid")] string groupid);

        [Delete("/ocs/v1.php/cloud/groups/{groupid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DeleteGroup([Header("Authorization")] string authorization,
                                                              [AliasAs("groupid")] string groupid);

        #endregion

        #region Apps

        [Get("/ocs/v1.php/cloud/apps?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetApps([Header("Authorization")] string authorization,
                                                          string filter);

        [Get("/ocs/v1.php/cloud/apps/{appid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> GetApp([Header("Authorization")] string authorization,
                                                         [AliasAs("appid")] string appid);

        [Post("/ocs/v1.php/cloud/apps/{appid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> EnableApp([Header("Authorization")] string authorization,
                                                            [AliasAs("appid")] string appid);

        [Delete("/ocs/v1.php/cloud/apps/{appid}?format=json")]
        Task<ApiResponse<OcsShareResponseSchema>> DisableApp([Header("Authorization")] string authorization,
                                                             [AliasAs("appid")] string appid);

        #endregion
    }
}