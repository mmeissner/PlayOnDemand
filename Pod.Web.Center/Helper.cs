#region Licence
/****************************************************************
 *  Filename: Helper.cs
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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pod.Data.Models.Shell;
using Pod.Services.Authentication;

namespace Pod.Web.Center
{
    public static class PodIdentityServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the identity system for the specified User and Role types.
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCore<TUser,TRole>(this IServiceCollection services) 
                where TUser : class
                where TRole : class
            => services.AddIdentityCore<TUser,TRole>(o => { });

        /// <summary>
        /// Adds and configures the identity system for the specified User and Role types.
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="IdentityOptions"/>.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCore<TUser,TRole>(
                this IServiceCollection services,
                Action<IdentityOptions> setupAction)
                where TUser : class
                where TRole : class
        {
            // Services identity depends on
            services.AddOptions().AddLogging();

            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();

            // Services used by identity
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
            services.TryAddScoped<UserManager<TUser>, UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>, SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>, AspNetRoleManager<TRole>>();

            if(setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser),typeof(TRole), services);
        }
    }

    public static class UserExtensions
    {
        /// <summary>
        /// Will be available if the User is authenticated by a JWT/Bearer authentication
        /// </summary>
        /// <returns>true if user Id was found</returns>
        public static bool GetUserId(this ClaimsPrincipal claimsPrincipal, out Guid userId)
        {
            return Guid.TryParse(claimsPrincipal.FindFirst(PodClaimsTypes.UserId)?.Value,out userId);
        }

        /// <summary>
        /// Will be available if the authentication was done with a <see cref="StationApiKey"/>
        /// </summary>
        /// <param name="claimsPrincipal">The Claims Principal set by the authentication</param>
        /// <param name="userId">The User Id from the claim</param>
        /// <param name="stationId">The Station Id from the claim</param>
        /// <returns>true if all values were found</returns>
        public static bool GetStationApiKeyData(this ClaimsPrincipal claimsPrincipal, out Guid userId, out Guid stationId)
        {
            stationId = Guid.Empty;
            return Guid.TryParse(claimsPrincipal.FindFirst(PodClaimsTypes.ApiKeyUserId)?.Value, out userId) &&
                   Guid.TryParse(claimsPrincipal.FindFirst(PodClaimsTypes.ApiKeyStationId)?.Value, out stationId);
        }
    }
}