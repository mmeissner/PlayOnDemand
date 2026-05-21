#region Licence
/****************************************************************
 *  Filename: ApiAccountExtensions.cs
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
using System.Text;
using Pod.DtoModels;
using Pod.ViewModels.Customer;
using Pod.ViewModels.User;
using RestSharp;

namespace Pod.Web.Client.Rest.Api.v1
{
    public static class ApiAccountExtensions
    {
        public static PodRestClient Account(this PodRestClient client) { return client; }

        /// <summary>
        /// Register a User Account
        /// </summary>
        public static IRestResponse Register(this PodRestClient podClient, RequestRegisterUserDto registerRequest)
        {
            return podClient.Execute(
                    new RestRequest("/api/v1/account/register", Method.POST, DataFormat.Json).AddJsonBody(
                            registerRequest));
        }

        /// <summary>
        /// Change the Password and get a new Refresh Token
        /// </summary>
        public static IRestResponse<ChangedPasswordUserViewModel> ChangePassword(this PodRestClient podClient, RequestChangePasswordDto changePasswordRequest)
        {
            return podClient.Execute<ChangedPasswordUserViewModel>(
                    new RestRequest("/api/v1/account/password/change", Method.POST, DataFormat.Json).AddJsonBody(
                            changePasswordRequest));
        }

        /// <summary>
        /// Receive a Reset Token to reset the Password
        /// </summary>
        public static IRestResponse ForgotPassword(this PodRestClient podClient, RequestForgotPasswordDto forgotPasswordRequest)
        {
            return podClient.Execute(
                    new RestRequest("/api/v1/account/password/forgot", Method.POST, DataFormat.Json).AddJsonBody(
                            forgotPasswordRequest));
        }

        /// <summary>
        /// Reset the Password with an Reset Token
        /// </summary>
        public static IRestResponse ForgotPassword(this PodRestClient podClient, RequestResetPasswordDto resetPasswordRequest)
        {
            return podClient.Execute(
                    new RestRequest("/api/v1/account/password/forgot", Method.POST, DataFormat.Json).AddJsonBody(
                            resetPasswordRequest));
        }

        /// <summary>
        /// Confirm an E-Mail with an E-Mail Confirmation Token
        /// </summary>
        public static IRestResponse ConfirmEmail(this PodRestClient podClient, RequestEmailConfirmationDto emailConfirmationRequest)
        {
            return podClient.Execute(
                    new RestRequest("/api/v1/account/password/forgot", Method.POST, DataFormat.Json).AddJsonBody(
                            emailConfirmationRequest));
        }

        ///// <summary>
        ///// Confirm an E-Mail with an E-Mail Confirmation Token
        ///// </summary>
        //public static IRestResponse ResendEMailConfirmation(this PodRestClient podClient, string username)
        //{
        //    return podClient.Execute(
        //            new RestRequest("/api/v1/account/password/forgot", Method.POST, DataFormat.Json).AddJsonBody(
        //                    EemailConfirmationRequest));
        //}

    }
}