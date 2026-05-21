#region Licence
/****************************************************************
 *  Filename: IntTestUserStationService.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.Services.Customer;
using Pod.Services.Station;
using Pod.Services.Test;
using Pod.Test.Utilities;
using Xunit;

namespace Pod.Data.Test
{
    [Collection("Database collection")]
    [TestCaseOrderer("Pod.Test.Utilities.PriorityOrderer", "Pod.Test.Utilities")]
    public class IntTestUserStationService : IClassFixture<ServicesFixture>
    {
        private readonly ServicesFixture _provider;
        public IntTestUserStationService(ServicesFixture provider)
        {
            _provider = provider;
            _provider.EnsureClearDb();
        }

        [Fact(Skip = "Depends on ServicesFixture (Postgres via PodDbContextFactory). To re-enable: rewrite ServicesFixture to use InMemory, or migrate this scenario into Pod.Web.Center.Test/Integration/ where PodWebApplicationFactory already provides an InMemory-backed PodDbContext."), TestPriority(1)]
        public async Task SessionViews()
        {
            var user = await _provider.CreateTestUser();
            var stationService = _provider.GetServiceProvider().GetService<StationService>();
            var station = await stationService.CreateNewStation(user.Id, "MyStation", "Password-1234");

            Assert.True(station.IsSuccess());
            using(var podContext = _provider.GetDbContext())
            {
                StartNewSession(podContext, station.ReturnValue.StationId);
            }

            var stationState = await _provider.GetServiceProvider().
                                               GetService<StationService>().
                                               GetStationCurrentState(user.Id, station.ReturnValue.StationId);

            Assert.True(stationState.IsSuccess());
            Assert.NotNull(stationState.ReturnValue);
            Assert.NotNull(stationState.ReturnValue.Session);
            Assert.NotNull(stationState.ReturnValue.Session.StartedOnUtc);
            Assert.Equal(SessionState.Started, stationState.ReturnValue.Session.State);
        }

        [Fact(Skip = "Depends on ServicesFixture (Postgres via PodDbContextFactory). To re-enable: rewrite ServicesFixture to use InMemory, or migrate this scenario into Pod.Web.Center.Test/Integration/ where PodWebApplicationFactory already provides an InMemory-backed PodDbContext."), TestPriority(2)]
        public async Task StationDisplayDetails()
        {
            string newStationName = "MyStation";
            var user = await _provider.CreateTestUser();
            var station = await _provider.GetServiceProvider().
                                          GetService<StationService>().
                                          CreateNewStation(user.Id, "MyStation", "Password-1234");

            Assert.True(station.IsSuccess());
            var stationSettings = await _provider.GetServiceProvider().
                                                        GetService<StationService>().
                                                        GetStationsDisplayDetails(user.Id);

            Assert.True(stationSettings.IsSuccess());
            int count = 0;
            foreach(var model in stationSettings.ReturnValue) count++;
            Assert.Equal(1, count);
            var stationSetting = stationSettings.ReturnValue.First();
            Assert.Equal(station.ReturnValue.StationId, stationSetting.StationId);
            Assert.Equal(newStationName, stationSetting.DisplayName);
            Assert.Equal(StationControlMode.Local, stationSetting.ControlMode);
            Assert.Null(stationSetting.QrCode);

            stationSetting.QrCode = "www.wechat.com/callmeback/{userId}";
            stationSetting.ControlMode = StationControlMode.RemoteWithQrCode;
            var changeResult = await _provider.GetServiceProvider().
                                               GetService<StationService>().
                                               SetStationSettings(
                                                       user.Id,
                                                       stationSetting.StationId,
                                                       stationSetting.DisplayName,
                                                       stationSetting.ControlMode,
                                                       stationSetting.QrCode);
            Assert.True(changeResult.IsSuccess());

            stationSetting.QrCode = null;
            changeResult = await _provider.GetServiceProvider().
                                           GetService<StationService>().
                                           SetStationSettings(user.Id,
                                                   stationSetting.StationId,
                                                   stationSetting.DisplayName,
                                                   stationSetting.ControlMode,
                                                   stationSetting.QrCode);
            Assert.False(changeResult.IsSuccess());
            Assert.True(changeResult.Any());
        }


        private Guid StartNewSession(PodDbContext podDbContext, Guid stationId)
        {
            var sourceIp = "1.1.1.1";
            var source = RequestSource.WebApi;
            var connectionId = Guid.NewGuid();

            var sessionDetails = GetSessionDetails(podDbContext, stationId);
            var requestSession = sessionDetails.RequestSession(source, sourceIp);
            Assert.True(requestSession.IsSuccess());
            Assert.True(requestSession.ReturnValue == SessionResponse.Success);
            podDbContext.SaveChanges();
            var requestDelivery = sessionDetails.RequestDelivery(connectionId);
            Assert.True(requestDelivery.IsSuccess());
            Assert.True(requestDelivery.ReturnValue == SessionResponse.Success);
            sessionDetails.SetResponse(connectionId, true);
            podDbContext.SaveChanges();
            return sessionDetails.Session.Id;
        }


        //Inline Function to get the SessionDetails for our test Station
        private SessionDetails GetSessionDetails(PodDbContext podDbContext, Guid stationId)
        {
            return podDbContext.SessionDetails.Where(x => x.StationId == stationId).
                                Include(x => x.Session).
                                Include(x => x.Station).
                                First();
        }
    }
}