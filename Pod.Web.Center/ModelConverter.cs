#region Licence
/****************************************************************
 *  Filename: ModelConverter.cs
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
using Pod.DtoModels;
using Pod.ViewModels.Customer;

namespace Pod.Web.Center
{
    public static class ModelConverter
    {

        public static StationSessionRequest ToStationSessionRequest(this RequestNewStationSessionDto request,Guid stationId)
        {
            return new StationSessionRequest()
                   {

                           StationId = stationId,
                           Reference = request.Reference,
                           Duration = request.Duration
            };
        }

        public static StationSessionUpdateRequest ToChangeRequest(this RequestSessionUpdateDto request)
        {
            return new StationSessionUpdateRequest
                   {
                           Reference = request.Reference,
                           Duration = request.Duration,
                   };
        }
    }
}
