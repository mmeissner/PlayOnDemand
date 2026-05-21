#region Licence
/****************************************************************
 *  Filename: UserViewModels.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Linq;
using System.Linq.Expressions;
using Pod.Data.Models.Users;
using Pod.ViewModels.Customer;

namespace Pod.ViewModels.Expressions
{

    public static class ToUserVm
    {
        public static Expression<Func<ApplicationUser, UserViewModel>> FromApplicationUser()
        {
            return x => new UserViewModel
                        {
                                Id = x.Id,
                                Username = x.UserName,
                                CustomerNumber = x.CustomerNumber,
                                EmailAddress = x.Email,
                                EmailConfirmed = x.EmailConfirmed,
                                IsLockedOut = x.LockoutEnabled,
                                //Do not optimize Count as it will otherwise not work 
                                StationCount = x.Stations != null ? x.Stations.Count() : 0,
                        };
            #region SQL Query
                //Microsoft.EntityFrameworkCore.Database.Command:Information: Executed DbCommand(2ms) [Parameters=[@__p_1='50', @__p_0='0'], CommandType='Text', CommandTimeout='30']
                //SELECT x."Id", x."UserName", x."CustomerNumber", x."Email" AS "EmailAddress", x."EmailConfirmed", x."LockoutEnabled" AS "IsLockedOut", CASE
                //        WHEN x."Id" IS NOT NULL
                //        THEN (
                //                SELECT COUNT(*)::INT4
                //FROM "Stations" AS s
                //WHERE x."Id" = s."ApplicationUserId"
                //        ) ELSE 0
                //END AS "StationCount"
                //FROM "Users" AS x
                //LIMIT @__p_1 OFFSET @__p_0
            #endregion
    }
    }
}
