#region Licence
/****************************************************************
 *  Filename: ApiStationsExtensions.cs
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
using Pod.DtoModels;
using Pod.Enums;
using Pod.ViewModels.Customer;
using RestSharp;

namespace Pod.Web.Client.Rest.Api.v1
{
    /// <summary>
    /// Station Api Requests implementation
    /// </summary>
    public static class ApiStationsExtensions
    {
        public static PodRestClient Stations(this PodRestClient client) { return client; }

        /// <summary>
        /// Requests the Station states
        /// </summary>
        /// <returns>A list with the current state of all stations</returns>
        public static IRestResponse<List<StationCurrentStateViewModel>> Get(this PodRestClient podClient)
        {
            return podClient.Execute<List<StationCurrentStateViewModel>>(
                    new RestRequest("/api/v1/Stations", Method.GET, DataFormat.Json));
        }

        /// <summary>
        /// Requests the state of a specific station
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <returns>The state of the station</returns>
        public static IRestResponse<StationCurrentStateViewModel> GetCurrentState(
                this PodRestClient podClient, string stationId)
        {
            return podClient.Execute<StationCurrentStateViewModel>(
                    new RestRequest($"/api/v1/Stations/{stationId}", Method.GET, DataFormat.Json));
        }

        /// <summary>
        /// Requests the current settings of all stations
        /// </summary>
        /// <returns>The current settings of all stations</returns>
        public static IRestResponse<List<StationSettingsViewModel>> GetDisplaySettings(this PodRestClient podClient)
        {
            return podClient.Execute<List<StationSettingsViewModel>>(
                    new RestRequest("/api/v1/Stations/DisplaySettings", Method.GET, DataFormat.Json));
        }

        /// <summary>
        /// Requests the current settings of a specific stations
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <returns>The current settings of a specific station</returns>
        public static IRestResponse<List<StationSettingsViewModel>>
                GetDisplaySettings(this PodRestClient podClient, string stationId)
        {
            return podClient.Execute<List<StationSettingsViewModel>>(
                    new RestRequest($"/api/v1/Stations/{stationId}/DisplaySettings", Method.GET, DataFormat.Json));
        }

        public static IRestResponse SetDisplaySettings(
                this PodRestClient podClient, string stationId, RequestStationSettingsDto settings)
        {
            return podClient.Execute(
                    new RestRequest($"/api/v1/Stations/{stationId}/DisplaySettings", Method.POST, DataFormat.Json).
                            AddJsonBody(settings));
        }

        /// <summary>
        /// Requests a Session History for a specific station
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="take">The max amount of return values</param>
        /// <param name="skip">The amount of return values to skip</param>
        /// <returns>List with Session Logs</returns>
        public static IRestResponse<List<SessionLogViewModel>> GetSession(
                this PodRestClient podClient, string stationId, int take = 0, int skip = 0)
        {
            var request = new RestRequest($"/api/v1/Stations/{stationId}/Session", Method.GET, DataFormat.Json);
            if(take > 0)
            {
                request.AddQueryParameter("take", take.ToString());
            }

            if(skip > 0)
            {
                request.AddQueryParameter("skip", skip.ToString());
            }

            return podClient.Execute<List<SessionLogViewModel>>(request);
        }

        /// <summary>
        /// Set a new QR Code for an station that will be displayed
        /// if the station is in RemoteModeWithQrCode
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="qrCode">The string that should be encoded in the QrCode</param>
        /// <returns></returns>
        public static IRestResponse SetQrCode(this PodRestClient podClient, string stationId, string qrCode)
        {
            return podClient.Execute(
                    new RestRequest(
                                    $"/api/v1/Stations/{stationId}/DisplaySettings/QrCode",
                                    Method.POST,
                                    DataFormat.Json).
                            AddJsonBody(
                                    new RequestStationQrCodeDto
                                    {
                                            QrCode = qrCode
                                    }));
        }

        /// <summary>
        /// Set the Mode for the Station
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="mode">The mode to set the station to</param>
        /// <returns></returns>
        public static IRestResponse SetMode(this PodRestClient podClient, string stationId, StationControlMode mode)
        {
            return podClient.Execute(
                    new RestRequest($"/api/v1/Stations/{stationId}/DisplaySettings/Mode", Method.POST, DataFormat.Json).
                            AddJsonBody(
                                    new RequestStationModeDto()
                                    {
                                            Mode = mode
                                    }));
        }

        /// <summary>
        /// Creates an new Station Account
        /// </summary>
        /// <param name="name">The display name for the station</param>
        /// <param name="password">The password for the station</param>
        /// <returns>The created station</returns>
        public static IRestResponse<StationCurrentStateViewModel> CreateStation(
                this PodRestClient podClient, string name, string password)
        {
            return podClient.Execute<StationCurrentStateViewModel>(
                    new RestRequest($"/api/v1/Stations/Create", Method.POST, DataFormat.Json).AddJsonBody(
                            new RequestCreateStationDto()
                            {
                                    DisplayName = name,
                                    Password = password
                            }));
        }

        /// <summary>
        /// Changes the Password for an Station
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="password">The new password</param>
        /// <returns></returns>
        public static IRestResponse SetPassword(this PodRestClient podClient, string stationId, string password)
        {
            return podClient.Execute(
                    new RestRequest(
                                    $"/api/v1/Stations/{stationId}/Password",
                                    Method.POST,
                                    DataFormat.Json).
                            AddJsonBody(
                                    new RequestSetStationPasswordDto()
                                    {
                                            Password = password
                                    }));
        }

        /// <summary>
        /// Requests the start of a new Gaming Session
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="reference">Custom identifier as string</param>
        /// <param name="duration">Null if there is no time limit or a timespan with the maximum runtime of the session</param>
        /// <returns></returns>
        public static IRestResponse SessionStart(this PodRestClient podClient, string stationId,string reference = null, TimeSpan? duration = null)
        {
            return podClient.Execute(
                    new RestRequest(
                                    $"/api/v1/Stations/{stationId}/Sessions/Start",
                                    Method.POST,
                                    DataFormat.Json).
                            AddJsonBody(
                                    new RequestNewStationSessionDto
                                    {
                                            Reference = reference,
                                            Duration = duration
                                    }));
        }

        /// <summary>
        /// Updates a sessions duration, or sets a new one if there was no max runtime set before
        /// </summary>
        /// <param name="stationId">The Id of the station</param>
        /// <param name="reference">Custom identifier as string</param>
        /// <param name="duration">The duration to add to the session</param>
        /// <returns></returns>
        public static IRestResponse SessionUpdate(this PodRestClient podClient, string stationId, string reference, TimeSpan duration)
        {
            return podClient.Execute(
                    new RestRequest(
                                    $"/api/v1/Stations/{stationId}/Sessions/Current/Update",
                                    Method.POST,
                                    DataFormat.Json).
                            AddJsonBody(
                                    new RequestSessionUpdateDto()
                                    {
                                            Reference = reference,
                                            Duration = duration
                                    }));
        }

        /// <summary>
        /// Requests to stop a session imminently 
        /// </summary>
        /// <param name="stationId">The Id of the station where the session is to be stopped</param>
        /// <returns></returns>
        public static IRestResponse SessionStop(this PodRestClient podClient, string stationId)
        {
            return podClient.Execute(
                    new RestRequest(
                                    $"/api/v1/Stations/{stationId}/Sessions/Current/Stop",
                                    Method.POST,
                                    DataFormat.Json));
        }
    }
}