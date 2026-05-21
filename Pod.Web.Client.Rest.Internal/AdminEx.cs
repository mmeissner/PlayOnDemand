#region Licence
/****************************************************************
 *  Filename: AdminEx.cs
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
using System.Collections.Generic;
using Pod.DtoModels;
using Pod.ViewModels.Admin;
using Pod.ViewModels.Mail;
using RestSharp;

namespace Pod.Web.Client.Rest.Internal
{
    public static class AdminEx
    {
        public static PodRestClient Admin(this PodRestClient client) { return client; }

        public static IRestResponse<List<UserRoleViewModel>> RolesGetAll(this PodRestClient podClient)
        {
            return podClient.Execute<List<UserRoleViewModel>>(
                    new RestRequest("api/v1/internal/Admin/roles", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<List<UserRoleViewModel>> RolesGetUser(this PodRestClient podClient, string username)
        {
            return podClient.Execute<List<UserRoleViewModel>>(
                    new RestRequest($"api/v1/internal/Admin/roles/{username}", Method.GET, DataFormat.Json));
        }

        public static IRestResponse RolesAddToUser(
                this PodRestClient podClient, RequestAddRemoveUserToRoleDto addRoleToUser)
        {
            return podClient.Execute<List<UserRoleViewModel>>(
                    new RestRequest($"api/v1/internal/Admin/roles/add", Method.POST, DataFormat.Json).AddJsonBody(
                            addRoleToUser));
        }

        public static IRestResponse RolesRemoveFromUser(
                this PodRestClient podClient, RequestAddRemoveUserToRoleDto removeRoleFromUser)
        {
            return podClient.Execute<List<UserRoleViewModel>>(
                    new RestRequest($"api/v1/internal/Admin/roles/remove", Method.POST, DataFormat.Json).AddJsonBody(
                            removeRoleFromUser));
        }

        public static IRestResponse<List<EMailAccountViewModel>> EmailAccountsGetAll(this PodRestClient podClient)
        {
            return podClient.Execute<List<EMailAccountViewModel>>(
                    new RestRequest("api/v1/internal/Email/accounts", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<EMailAccountViewModel> EmailAccountsCreate(
                this PodRestClient podClient, RequestCreateEmailAccountDto newEmailAccount)
        {
            return podClient.Execute<EMailAccountViewModel>(
                    new RestRequest($"api/v1/internal/Email/accounts", Method.POST, DataFormat.Json).AddJsonBody(
                            newEmailAccount));
        }

        public static IRestResponse<EMailAccountViewModel> EmailAccountsEnable(
                this PodRestClient podClient, long accountId, bool isEnabled)
        {
            return podClient.Execute<EMailAccountViewModel>(
                    new RestRequest($"api/v1/internal/Email/accounts/{accountId}/enable", Method.POST, DataFormat.Json).
                            AddQueryParameter(nameof(isEnabled), isEnabled.ToString()));
        }

        public static IRestResponse<EMailAccountViewModel> EmailAccountsSetAuth(
                this PodRestClient podClient, long accountId, RequestSetSmtpAuthSettingsDto authSettings)
        {
            return podClient.Execute<EMailAccountViewModel>(
                    new RestRequest($"api/v1/internal/Email/accounts/{accountId}/auth", Method.POST, DataFormat.Json).
                            AddJsonBody(authSettings));
        }

        public static IRestResponse<EMailAccountViewModel> EmailAccountsSetSmtp(
                this PodRestClient podClient, long accountId, RequestSetSmtpServerDto smtpSettings)
        {
            return podClient.Execute<EMailAccountViewModel>(
                    new RestRequest($"api/v1/internal/Email/accounts/{accountId}/smtp", Method.POST, DataFormat.Json).
                            AddJsonBody(smtpSettings));
        }

        public static IRestResponse<EMailAccountViewModel> EmailAccountsSetSender(
                this PodRestClient podClient, long accountId, RequestSetEmailSenderDto senderSettings)
        {
            return podClient.Execute<EMailAccountViewModel>(
                    new RestRequest($"api/v1/internal/Email/accounts/{accountId}/sender", Method.POST, DataFormat.Json).
                            AddJsonBody(senderSettings));
        }

        public static IRestResponse EmailAccountsSendMail(
                this PodRestClient podClient, long accountId, RequestSendEmailDto sendMailRequest)
        {
            return podClient.Execute(
                    new RestRequest(
                            $"api/v1/internal/Email/accounts/{accountId}/sendMail",
                            Method.POST,
                            DataFormat.Json).AddJsonBody(sendMailRequest));
        }

        public static IRestResponse EmailAccountsDelete(this PodRestClient podClient, long accountId)
        {
            return podClient.Execute(
                    new RestRequest($"api/v1/internal/Email/accounts/{accountId}", Method.DELETE, DataFormat.Json));
        }

        public static IRestResponse EmailAccountsAddTemplate(
                this PodRestClient podClient, long accountId, long templateId)
        {
            return podClient.Execute(
                    new RestRequest(
                            $"api/v1/internal/Email/accounts/{accountId}/templates/add/{templateId}",
                            Method.POST,
                            DataFormat.Json));
        }

        public static IRestResponse EmailAccountsRemoveTemplate(
                this PodRestClient podClient, long accountId, long templateId)
        {
            return podClient.Execute(
                    new RestRequest(
                            $"api/v1/internal/Email/accounts/{accountId}/templates/remove/{templateId}",
                            Method.POST,
                            DataFormat.Json));
        }

        public static IRestResponse<EmailTemplateInfo> EmailTemplatesGetInfo(this PodRestClient podClient)
        {
            return podClient.Execute<EmailTemplateInfo>(
                    new RestRequest("api/v1/internal/Email/templates/info", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<List<EMailTemplateDetailsViewModel>> EmailTemplatesGetAll(
                this PodRestClient podClient)
        {
            return podClient.Execute<List<EMailTemplateDetailsViewModel>>(
                    new RestRequest("api/v1/internal/Email/templates", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<EMailTemplateDetailsViewModel> EmailTemplatesCreate(
                this PodRestClient podClient, RequestCreateEmailTemplateDto newTemplate)
        {
            return podClient.Execute<EMailTemplateDetailsViewModel>(
                    new RestRequest($"api/v1/internal/Email/templates", Method.POST, DataFormat.Json).AddJsonBody(
                            newTemplate));
        }

        public static IRestResponse EmailTemplatesDelete(this PodRestClient podClient, long templateId)
        {
            return podClient.Execute(
                    new RestRequest($"api/v1/internal/Email/templates/{templateId}", Method.DELETE, DataFormat.Json));
        }
    }
}