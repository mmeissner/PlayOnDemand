#region Licence
/****************************************************************
 *  Filename: ServerEx.cs
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
using Pod.ViewModels.ShellServer;
using RestSharp;

namespace Pod.Web.Client.Rest.Internal {
    public static class ServerEx
    {
        public static PodRestClient Servers(this PodRestClient client) { return client; }

        public static IRestResponse<List<ShellServerDetailsViewModel>> Get(this PodRestClient podClient)
        {
            return podClient.Execute<List<ShellServerDetailsViewModel>>(
                    new RestRequest("api/v1/internal/Server", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<ShellServerDetailsViewModel> Get(this PodRestClient podClient, long serverId)
        {
            return podClient.Execute<ShellServerDetailsViewModel>(
                    new RestRequest($"api/v1/internal/Server/{serverId}", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<List<ShellServerConnectedClientViewModel>> GetConnectedStations(
                this PodRestClient podClient, long serverId)
        {
            return podClient.Execute<List<ShellServerConnectedClientViewModel>>(
                    new RestRequest($"api/v1/internal/Server/{serverId}/stations", Method.GET, DataFormat.Json));
        }

        public static IRestResponse<ShellServerViewModel> Create(
                this PodRestClient podClient, RequestNewServerDto newServer)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server", Method.POST, DataFormat.Json).
                            AddJsonBody(newServer));
        }

        public static IRestResponse<ShellServerViewModel> SetDisplayName(
                this PodRestClient podClient, long serverId, RequestServerDisplayNameUpdateDto displayNameUpdate)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server/{serverId}/displaySettings", Method.POST, DataFormat.Json).
                            AddJsonBody(displayNameUpdate));
        }

        public static IRestResponse<ShellServerViewModel> SetConnectionSettings(
                this PodRestClient podClient, long serverId,
                RequestServerConnectionSettingsUpdateDto connectionSettings)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server/{serverId}/connection", Method.POST, DataFormat.Json).
                            AddJsonBody(connectionSettings));
        }

        public static IRestResponse<ShellServerViewModel> SetTimeSettings(
                this PodRestClient podClient, long serverId, RequestServerTimeSettingsUpdateDto timeSettingsUpdate)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server/{serverId}/timing", Method.POST, DataFormat.Json).
                            AddJsonBody(timeSettingsUpdate));
        }

        public static IRestResponse<ShellServerViewModel> SetEnabled(
                this PodRestClient podClient, long serverId, bool isEnabled)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server/{serverId}/enabled", Method.POST, DataFormat.Json).
                            AddQueryParameter(nameof(isEnabled), isEnabled.ToString()));
        }
        public static IRestResponse<ShellServerViewModel> Shutdown(this PodRestClient podClient)
        {
            return podClient.Execute<ShellServerViewModel>(
                    new RestRequest($"api/v1/internal/Server/shutdown", Method.POST, DataFormat.Json));
        }
    }
}