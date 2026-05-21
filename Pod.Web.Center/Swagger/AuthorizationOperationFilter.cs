#region Licence
/****************************************************************
 *  Filename: AuthorizationOperationFilter.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Pod.Web.Center.Swagger
{
    /// <summary>
    /// Filters Routes for the Authorize attribute and applies the authentication scheme and response for doc
    /// </summary>
    public class AuthorizationOperationFilter : IOperationFilter
    {
        private readonly string _securitySchemaName;
        private readonly SecurityRequirementsOperationFilter<AuthorizeAttribute> filter;

        public AuthorizationOperationFilter(
                string securitySchemaName, bool includeUnauthorizedAndForbiddenResponses = true)
        {
            _securitySchemaName = securitySchemaName;
            this.filter = new SecurityRequirementsOperationFilter<AuthorizeAttribute>(
                    authAttributes => authAttributes.
                                      Where(a => !string.IsNullOrEmpty(a.Policy)).
                                      Select(a => a.Policy),
                    includeUnauthorizedAndForbiddenResponses,
                    securitySchemaName);
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if(GetControllerAndActionAttributes<AllowAnonymousAttribute>(context).Any()) return;
            IEnumerable<AuthorizeAttribute> actionAttributes =
                    GetControllerAndActionAttributes<AuthorizeAttribute>(context);
            var authorizeAttributes = actionAttributes as AuthorizeAttribute[] ?? actionAttributes.ToArray();
            if(!authorizeAttributes.Any()) return;

            var attribute = authorizeAttributes.FirstOrDefault(
                    x => x.AuthenticationSchemes != null &&
                         x.AuthenticationSchemes.Equals(
                                 _securitySchemaName,
                                 StringComparison.CurrentCultureIgnoreCase));
            if(attribute != null)
            {
                this.filter.Apply(operation, context);
            }
        }

        static IEnumerable<T> GetControllerAndActionAttributes<T>(
                OperationFilterContext context)
                where T : Attribute
        {
            IEnumerable<T> customAttributes1 = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes<T>();
            IEnumerable<T> customAttributes2 = context.MethodInfo.GetCustomAttributes<T>();
            List<T> objList = new List<T>(customAttributes1);
            objList.AddRange(customAttributes2);
            return (IEnumerable<T>)objList;
        }
    }
}
