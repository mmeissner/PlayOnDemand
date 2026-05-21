#region Licence
/****************************************************************
 *  Filename: SupportEx.cs
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
using Pod.ViewModels.Customer;
using RestSharp;

namespace Pod.Web.Client.Rest.Internal {
    public static class SupportEx
    {
        public static PodRestClient Support(this PodRestClient client) { return client; }

        public static IRestResponse<List<UserViewModel>> UsersGetAll(
                this PodRestClient podClient, int skip = 0, int take = 50)
        {
            return podClient.Execute<List<UserViewModel>>(
                    new RestRequest("api/v1/internal/support/users", Method.GET, DataFormat.Json).
                            AddQueryParameter(nameof(take),take.ToString()).
                            AddQueryParameter(nameof(skip),skip.ToString()));
        }

        public static IRestResponse<List<UserViewModel>> UsersGetByEmail(
                this PodRestClient podClient,string emailAddress)
        {
            return podClient.Execute<List<UserViewModel>>(
                    new RestRequest("api/v1/internal/support/users/email", Method.GET, DataFormat.Json).
                            AddQueryParameter(nameof(emailAddress),emailAddress));
        }

        public static IRestResponse<List<StationCurrentStateViewModel>> UsersGetStations(this PodRestClient podClient, Guid userId)
        {
            return podClient.Execute<List<StationCurrentStateViewModel>>(
                    new RestRequest($"api/v1/internal/support/users/{userId}/stations", Method.GET, DataFormat.Json));
        }
        public static IRestResponse<List<StationConnectionLogViewModel>> StationsGetConnections(this PodRestClient podClient, Guid stationId)
        {
            return podClient.Execute<List<StationConnectionLogViewModel>>(
                    new RestRequest($"api/v1/internal/support/stations/{stationId}/connections", Method.GET, DataFormat.Json));
        }
    }
}