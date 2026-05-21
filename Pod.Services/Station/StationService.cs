#region Licence
/****************************************************************
 *  Filename: StationService.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.Services.System;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Expressions;

namespace Pod.Services.Station
{
    /// <summary>
    /// Service for Users to manage stations and sessions
    /// </summary>
    public class StationService
    {
        private readonly ILogger<StationService> _logger;
        private readonly PublisherHub<ClientCommandType> _publisherHub;
        private readonly StationResponseHub _stationResponseHub;
        private readonly SystemSettingsService _settingsService;
        private readonly PodDbContext _podContext;
        public StationService(
                ILogger<StationService> logger,
                PublisherHub<ClientCommandType> publisherHub,
                StationResponseHub stationResponseHub,
                SystemSettingsService settingsService,
                PodDbContext podContext)
        {
            _logger = logger;
            _publisherHub = publisherHub;
            _stationResponseHub = stationResponseHub;
            _settingsService = settingsService;
            _podContext = podContext;
        }

        /// <summary>
        /// Get the Users Stations with their current State
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <returns>Collection with Stations States</returns>
        public async Task<IResult<IEnumerable<StationCurrentStateViewModel>>> GetStationsCurrentState(
                Guid userId, NetworkState? state = null)
        {
            var result = new Result<IEnumerable<StationCurrentStateViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if(result.HasError()) return result;
            return result.Add(
                    (await _podContext.Stations.
                                       Where(
                                               x => x.ApplicationUserId == userId &&
                                                    (state == null || x.ConnectionState.NetworkState == state.Value)).
                                       OrderBy(x => x.CreatedOnUtc).
                                       Select(ToStationCurrentStateVm.FromStation()).
                                       AsNoTracking().
                                       ToListAsync()));
        }

        /// <summary>
        /// Get the Station State for a specific Station/User
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <param name="stationId">The StationId</param>
        /// <returns>The Station State </returns>
        public async Task<IResult<StationCurrentStateViewModel>> GetStationCurrentState(Guid userId, Guid stationId)
        {
            var result = new Result<StationCurrentStateViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(
                    stationId,
                    nameof(Data.Models.Shell.Station),
                    Guid.Empty,
                    UserError.StationInvalidStationId);
            if(result.HasError()) return result;
            var searchResult = await _podContext.Stations.
                                                 Where(x => x.ApplicationUserId == userId && x.Id == stationId).
                                                 Select(ToStationCurrentStateVm.FromStation()).
                                                 AsNoTracking().
                                                 FirstOrDefaultAsync();
            result.ArgNotNull(searchResult, nameof(searchResult), UserError.StationNotFound);
            return result.Add(searchResult);
        }
        
        /// <summary>
        /// Get the Display Details for all Stations of a User 
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <returns>Collection with all Users Station Details</returns>
        public async Task<IResult<IEnumerable<StationSettingsViewModel>>> GetStationsDisplayDetails(Guid userId)
        {
            var result = new Result<IEnumerable<StationSettingsViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId));
            if(result.HasError()) return result;
            return result.Add(
                    await _podContext.Stations.
                                      Where(x => x.ApplicationUserId == userId).
                                      OrderBy(x => x.CreatedOnUtc).
                                      Select(ToStationSettingsVm.FromStation()).
                                      AsNoTracking().
                                      ToArrayAsync());
        }

        /// <summary>
        /// Get the Display Details for a Station of a User
        /// </summary>
        /// <param name="userId">The Users Id</param>
        /// <param name="stationId">The Station Id</param>
        /// <returns></returns>
        public async Task<IResult<StationSettingsViewModel>> GetStationsDisplayDetails(Guid userId, Guid stationId)
        {
            var result = new Result<StationSettingsViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEmpty(stationId, nameof(stationId), UserError.StationInvalidStationId);
            if(result.HasError()) return result;
            return result.Add(
                    await _podContext.Stations.
                                      Where(x => x.ApplicationUserId == userId && x.Id == stationId).
                                      OrderBy(x => x.CreatedOnUtc).
                                      Select(ToStationSettingsVm.FromStation()).
                                      AsNoTracking().
                                      FirstOrDefaultAsync());
        }

        /// <summary>
        /// Creates a new Station
        /// </summary>
        /// <param name="userId">The Users Id</param>
        /// <param name="displayName">The Name of the Station</param>
        /// <param name="password">The Password for the Station</param>
        /// <returns>The created Station</returns>
        public async Task<IResult<StationSettingsViewModel>> CreateNewStation(
                Guid userId, string displayName, string password)
        {
            var result = new Result<StationSettingsViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotNullOrWhitespace(displayName, nameof(displayName));
            ValidateStationPassword(result, password);
            if(result.HasError()) return result;

            //Check the max amount of stations per user
            if(_podContext.Stations.Where(x => x.ApplicationUserId == userId).Count() >=
               _settingsService.GetSystemSettings.MaxStationsPerUser)
            {
                return result.Add(
                        "Maximum allowed amount of stations reached!",
                        UserError.StationMaxAmountReached);
            }

            //Create a new Station
            var newStationResult = Data.Models.Shell.Station.Create(
                    userId,
                    displayName,
                    password,
                    new PasswordHasher());

            //Check Result of create
            if(result.Add(newStationResult).HasError()) return result;

            //Save new Station
            _podContext.Add(newStationResult.ReturnValue);
            await _podContext.SaveChangesAsync();
            return result.Add(ToStationSettingsVm.FuncFromStation(newStationResult.ReturnValue));
        }

        /// <summary>
        /// Get finished Sessions for a Station of a User
        /// </summary>
        /// <param name="userId">The Users Id</param>
        /// <param name="stationId">The Stations Id</param>
        /// <param name="take">The amount of results</param>
        /// <param name="skip">The amount of results to skip</param>
        /// <returns></returns>
        public async Task<IResult<IEnumerable<SessionLogViewModel>>> GetStationsSessionLogs(
                Guid userId, Guid stationId, int take = 50, int skip = 0)
        {
            var result = new Result<IEnumerable<SessionLogViewModel>>();
            if(!result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId) ||
               !result.ArgNotEmpty(stationId, nameof(stationId), UserError.StationInvalidStationId) ||
               !result.ValueNotNull(
                       await _podContext.Stations.
                                         Where(x => x.Id == stationId && x.ApplicationUserId == userId).
                                         FirstOrDefaultAsync(),
                       "result of query for UserId with specific Station Id",
                       UserError.StationInvalidStationId))
            {
                return result;
            }

            return result.Add(
                    (await _podContext.Sessions.
                                       Where(x => x.StationId == stationId && x.IsClosed).
                                       Include(x => x.ChangeRequests).
                                       OrderBy(x => x.RequestedOnUtc).
                                       Skip(skip).
                                       Take(take).
                                       AsNoTracking().
                                       ToArrayAsync()).Select(ToSessionLogVm.FuncFromSession));
        }
        
        /// <summary>
        /// Get finished Sessions for a Station of a User
        /// </summary>
        /// <param name="userId">The Users Id</param>
        /// <param name="take">The amount of results</param>
        /// <param name="skip">The amount of results to skip</param>
        /// <returns></returns>
        public async Task<IResult<IEnumerable<SessionLogViewModel>>> GetStationsSessionLogs(
                Guid userId, int take = 50, int skip = 0)
        {
            var result = new Result<IEnumerable<SessionLogViewModel>>();
            if(!result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId))
            {
                return result;
            }

            return result.Add(
                    (await _podContext.Sessions.
                                       Where(x => x.Station.ApplicationUserId == userId).
                                       OrderBy(x => x.RequestedOnUtc).
                                       Skip(skip).
                                       Take(take).
                                       Include(x => x.ChangeRequests).
                                       AsNoTracking().
                                       ToArrayAsync()).Select(ToSessionLogVm.FuncFromSession));
        }

        /// <summary>
        /// Sets the Display Details of a Station and verifies if the Station and User belong together
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <param name="stationId">The StationId</param>
        /// <param name="displayName">The display name to set</param>
        /// <param name="mode">The control mode to set</param>
        /// <param name="qrCode">The QrCode to set</param>
        /// <returns>The Result</returns>
        public async Task<IResult> SetStationSettings(
                Guid userId, Guid stationId, string displayName, StationControlMode mode, string qrCode)
        {
            var result = new Result();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if(result.HasError()) return result;
            var dbDisplayDetails = await _podContext.StationSettings.
                                                     Where(
                                                             x => x.Station.Id == stationId &&
                                                                  x.Station.ApplicationUserId == userId).
                                                     FirstOrDefaultAsync();
            return await SetStationSettings(dbDisplayDetails, displayName, mode, qrCode);
        }

        /// <summary>
        /// Sets the Display Details of a Station
        /// </summary>
        /// <param name="dbDisplayDetails">The object where to make the changes</param>
        /// <param name="displayName">The display name to set</param>
        /// <param name="mode">The control mode to set</param>
        /// <param name="qrCode">The QrCode to set</param>
        /// <returns>The Result</returns>
        private async Task<IResult> SetStationSettings(
                StationSettings dbDisplayDetails, string displayName, StationControlMode mode, string qrCode)
        {
            var result = new Result();
            if (!result.ValueNotNull(
                    dbDisplayDetails,
                    nameof(StationSettings),
                    UserError.ShellServerInvalidId)) return result;

            result.Add(dbDisplayDetails.SetQrCodeAndControlMode(qrCode, mode));
            result.Add(dbDisplayDetails.SetDisplayName(displayName));

            if (result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
                _publisherHub.Publish(dbDisplayDetails.StationId, ClientCommandType.UpdateClientSettings);
            }

            return result;
        }

        /// <summary>
        /// Sets the string for a QR-Code on an station and verifies if the User is the owner
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <param name="stationId">The Station Id</param>
        /// <param name="qrCode">The string for the QRCode</param>
        /// <returns></returns>
        public async Task<IResult> SetStationQrCode(Guid userId, Guid stationId, string qrCode)
        {
            var result = new Result();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if(result.HasError()) return result;
            var dbDisplayDetails = await _podContext.StationSettings.
                                                     Where(
                                                             x => x.Station.Id == stationId &&
                                                                  x.Station.ApplicationUserId == userId).
                                                     SingleOrDefaultAsync();
            return await SetStationQrCode(dbDisplayDetails, qrCode);
        }

        /// <summary>
        /// Sets the string for a QR-Code on an station with an ApiPublicKey
        /// </summary>
        /// <param name="apiPublicKey">The ApiPublicKey for the station</param>
        /// <param name="qrCode">The string for the QRCode</param>
        /// <returns></returns>

        private async Task<IResult> SetStationQrCode(StationSettings dbDisplayDetails, string qrCode)
        {
            var result = new Result();
            if (!result.ValueNotNull(
                    dbDisplayDetails,
                    nameof(StationSettings),
                    UserError.ShellServerInvalidId)) return result;

            result.Add(dbDisplayDetails.SetQrCode(qrCode));

            if (result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
                _publisherHub.Publish(dbDisplayDetails.StationId, ClientCommandType.UpdateClientSettings);
            }
            return result;
        }

        /// <summary>
        /// Sets the Operational Mode for a Station and verifies if the station belongs to the userid
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="stationId">The Station Id</param>
        /// <param name="mode">The Mode to set</param>
        /// <returns></returns>
        public async Task<IResult> SetStationMode(Guid userId, Guid stationId, StationControlMode mode)
        {
            var result = new Result();
            result.ArgNotNullOrEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if(result.HasError()) return result;
            var dbDisplayDetails = await _podContext.StationSettings.
                                                     Where(
                                                             x => x.Station.Id == stationId &&
                                                                  x.Station.ApplicationUserId == userId).
                                                     SingleOrDefaultAsync();

            return await SetStationMode(dbDisplayDetails, mode);
        }

        /// <summary>
        /// Sets the Operational Mode for a Station
        /// </summary>
        /// <param name="dbDisplayDetails">The User Settings were to set the mode</param>
        /// <param name="mode">The Mode to set</param>
        /// <returns></returns>
        private async Task<IResult> SetStationMode(StationSettings dbDisplayDetails, StationControlMode mode)
        {
            var result = new Result();
            if(!result.ValueNotNull(
                    dbDisplayDetails,
                    nameof(StationSettings),
                    UserError.ShellServerInvalidId)) return result;
            result.Add(dbDisplayDetails.SetControlMode(mode));
            if(result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
                _publisherHub.Publish(dbDisplayDetails.StationId, ClientCommandType.UpdateClientSettings);
            }

            return result;
        }


        /// <summary>
        /// Sets a Station Password 
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="stationId">The Station Id</param>
        /// <param name="password">The new Password</param>
        /// <returns>The Result</returns>
        public async Task<IResult> SetStationPassword(Guid userId, Guid stationId, string password)
        {
            var result = new Result();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            ValidateStationPassword(result, password);
            if(result.HasError()) return result;
            var station = await _podContext.Stations.Where(x => x.Id == stationId && x.ApplicationUserId == userId).
                                            SingleOrDefaultAsync();
            if(!result.ArgNotNull(station, nameof(station),UserError.StationNotFound)) return result;
            result.Add(station.SetPassword(password, new PasswordHasher()));
            if(result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
                //If we changed the password and the Station is connected we should disconnect it and remove it from the notifications
                _publisherHub.Publish(stationId, ClientCommandType.Disconnect);
            }
            return result;
        }
        
        /// <summary>
        /// Requests a new Session to start and verifies if the user belongs to the station
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="requestPeer">The Peer the request is send from</param>
        /// <param name="sessionRequest">The Session information</param>
        /// <returns>The Result</returns>
        public async Task<IResult<CreatedSessionViewModel>> RequestStationSession(
                Guid userId, string requestPeer, StationSessionRequest sessionRequest)
        {
            var result = new Result<CreatedSessionViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(sessionRequest.StationId, nameof(sessionRequest.StationId), Guid.Empty);
            if(result.HasError()) return result;
            var station = await _podContext.Stations.
                                            Where(
                                                    x => x.Id == sessionRequest.StationId &&
                                                         x.ApplicationUserId == userId).
                                            Include(x => x.SessionDetails).
                                            ThenInclude(x => x.Session).
                                            Include(x => x.ConnectionState).
                                            FirstAsync();

            return await RequestStationSession(station, requestPeer, sessionRequest);
        }
        
        /// <summary>
        /// Requests a new Session to start
        /// </summary>
        /// <param name="station">The Station were to request the session</param>
        /// <param name="requestPeer">The Peer the request is send from</param>
        /// <param name="sessionRequest">The Session information</param>
        /// <returns>The Result</returns>
        private async Task<IResult<CreatedSessionViewModel>> RequestStationSession(
                Data.Models.Shell.Station station, string requestPeer, StationSessionRequest sessionRequest)
        {
            var result = new Result<CreatedSessionViewModel>();
            result.ArgNotNull(station, nameof(station), UserError.StationNotFound);
            if(result.HasError()) return result;

            //Check if station is connected and responding
            DateTime expectResponseNotBefore = DateTime.UtcNow;
            if(station.ConnectionState.NetworkState != NetworkState.Connected ||
               !_publisherHub.Publish(sessionRequest.StationId, ClientCommandType.SendHeartbeat))
            {
                result.Add("Client is not connected", UserError.StationIsNotConnected);
            }
            else
            {
                var response = await _stationResponseHub.WaitForResponse(
                        sessionRequest.StationId,
                        ClientRequestType.SetHeartbeat,
                        expectResponseNotBefore,
                        TimeSpan.FromSeconds(5));
                if(response.IsTimeoutResponse)
                {
                    result.Add("Client did not respond in Time!", UserError.StationDidNotRespondInTime);
                }
                else if(response.ResponseForClient.HasError())
                {
                    result.Add("Error occurred during Heartbeat on Client side", UserError.StationErrorOccured);
                }
            }

            if(result.HasError()) return result;

            //Request Session
            var resultRequestSession = station.SessionDetails.RequestSession(
                    RequestSource.WebApi,
                    requestPeer,
                    sessionRequest.Reference);
            if(resultRequestSession.HasError()) return result.Add(resultRequestSession);

            //Do not continue if client did not respond
            if(!result.IsSessionResponseSuccess(resultRequestSession.ReturnValue) ||
               //Or has a limit to set (duration) and setting this limit returned an error
               sessionRequest.Duration.HasValue &&
               result.Add(station.SessionDetails.Session.AddStartCondition(sessionRequest.Duration.Value)).HasError())
                return result;

            //Save when everything went well
            await _podContext.SaveChangesAsync();

            //Inform Client about the new Login Request
            _publisherHub.Publish(sessionRequest.StationId, ClientCommandType.GetLoginRequest);

            //Return Info about the new Session
            if(station.SessionDetails.SessionId.HasValue)
            {
                result.Add(
                        new CreatedSessionViewModel()
                        {
                                StationId = sessionRequest.StationId,
                                SessionId = station.SessionDetails.SessionId.Value
                        });
            }
            else
            {
                result.Add("Could not get the SessionId for newly created Session", UserError.InternalError);
            }

            return result;
        }

        /// <summary>
        /// Requests an Update to the current running Session and verifies if the user is the owner
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="stationId">The Station Id</param>
        /// <param name="requestPeer">The peer the request is send from</param>
        /// <param name="sessionUpdateRequest">The Update information</param>
        /// <returns>The Result</returns>
        public async Task<IResult<UpdatedSessionViewModel>> RequestStationSessionChange(
                Guid userId, Guid stationId, string requestPeer, StationSessionUpdateRequest sessionUpdateRequest)
        {
            var result = new Result<UpdatedSessionViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if(result.HasError()) return result;
            var station = await _podContext.Stations.
                                            Where(x => x.Id == stationId && x.ApplicationUserId == userId).
                                            Include(x => x.SessionDetails).
                                            ThenInclude(x => x.Session).
                                            ThenInclude(x => x.ChangeRequests).
                                            FirstAsync();
            return await RequestStationSessionChange(station, requestPeer, sessionUpdateRequest);
        }

        /// <summary>
        /// Requests an Update to the current running Session
        /// </summary>
        /// <param name="station">The station the update is for</param>
        /// <param name="requestPeer">The peer the request is send from</param>
        /// <param name="sessionUpdateRequest">The Update information</param>
        /// <returns>The Result</returns>
        private async Task<IResult<UpdatedSessionViewModel>> RequestStationSessionChange(
                Data.Models.Shell.Station station, string requestPeer, StationSessionUpdateRequest sessionUpdateRequest)
        {
            var result = new Result<UpdatedSessionViewModel>();
            result.ArgNotNull(station, nameof(station), UserError.StationNotFound);
            if(result.HasError()) return result;
            var resultRequestResponse = station.SessionDetails.RequestSessionChange(
                    RequestSource.WebApi,
                    requestPeer,
                    sessionUpdateRequest.Duration,
                    sessionUpdateRequest.Reference);
            //Handle the Response and check for errors
            if(resultRequestResponse.HasError()) return result.Add(resultRequestResponse);
            if(!result.IsSessionResponseSuccess(resultRequestResponse.ReturnValue.Response)) return result;
            await _podContext.SaveChangesAsync();
            result.Add(
                    new UpdatedSessionViewModel()
                    {
                            StationId = station.Id,
                            SessionId = station.SessionDetails.SessionId.Value,
                            ChangeRequestId = resultRequestResponse.ReturnValue.ChangeRequest.Id,
                    });
            _publisherHub.Publish(station.Id, ClientCommandType.UpdateSession);
            return result;
        }

        /// <summary>
        /// Requests the current Session to stop and verifies if the UserId belongs to the StationId
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="stationId">The Station Id</param>
        /// <returns>The Result</returns>
        public async Task<IResult<StoppedSessionViewModel>> RequestStationSessionStop(Guid userId, Guid stationId)
        {
            var result = new Result<StoppedSessionViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            var station = await _podContext.Stations.
                                            Where(x => x.Id == stationId && x.ApplicationUserId == userId).
                                            Include(x => x.SessionDetails).
                                            ThenInclude(x => x.Session).
                                            FirstAsync();
            return await RequestStationSessionStop(station);
        }

        /// <summary>
        /// Requests the current Session to stop 
        /// </summary>
        /// <param name="station">The Station where to stop the session</param>
        /// <returns>The Result</returns>
        private async Task<IResult<StoppedSessionViewModel>> RequestStationSessionStop(
                Data.Models.Shell.Station station)
        {
            var result = new Result<StoppedSessionViewModel>();
            if(result.ArgNotNull(station, nameof(station), UserError.StationNotFound) &&
               result.ArgNotNull(
                       station.SessionDetails.Session,
                       nameof(station.SessionDetails.Session),
                       UserError.SessionNotFound) &&
               result.ArgIsEnum(
                       typeof(SessionState),
                       station.SessionDetails.Session.State,
                       SessionState.Started,
                       nameof(station.SessionDetails.Session.State),
                       UserError.SessionIsNotStarted))
            {
                // ReSharper disable once PossibleInvalidOperationException
                var currentSessionId = station.SessionDetails.SessionId.Value;

                var resultRequestResponse = station.SessionDetails.EndSession(StopReason.RemoteLogout);
                if(resultRequestResponse.HasError()) return result.Add(resultRequestResponse);
                if(!result.IsSessionResponseSuccess(resultRequestResponse.ReturnValue)) return result;
                await _podContext.SaveChangesAsync();
                result.Add(
                        new StoppedSessionViewModel()
                        {
                                StationId = station.Id,
                                SessionId = currentSessionId
                        });
                _publisherHub.Publish(station.Id, ClientCommandType.UpdateSession);
            }

            return result;
        }


        private async Task<Result> CheckForValidSubscription(Result currentResult, Guid stationId)
        {
            var subscription =
                    await _podContext.SubscriptionStates.Where(x => x.StationId == stationId).FirstOrDefaultAsync();
            currentResult.RefNotNull(subscription, nameof(subscription));
            currentResult.RefNotNull(
                    subscription?.ExpiresOnUtc,
                    nameof(subscription.ExpiresOnUtc),
                    UserError.StationNoValidSubscription);
            if(currentResult.HasError()) return currentResult;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once PossibleInvalidOperationException
            currentResult.ArgNotAfterOrEqualThen(
                    subscription.ExpiresOnUtc.Value,
                    nameof(subscription.ExpiresOnUtc.Value),
                    DateTime.UtcNow,
                    nameof(DateTime.UtcNow),
                    UserError.StationNoValidSubscription);
            return currentResult;
        }

        /// <summary>
        /// Validates if a Station Password fulfills the security requirements
        /// </summary>
        /// <param name="result"></param>
        /// <param name="password"></param>
        private void ValidateStationPassword(Result result, string password)
        {
            result.StringNotShorterThen(password, nameof(password), 10, UserError.StationPasswordTooShort);
            result.StringNotLongerThen(password, nameof(password), 128, UserError.StationPasswordTooLong);
            result.StringMustContainLowerCase(password, nameof(password), UserError.StationPasswordHasNoLowerChars);
            result.StringMustContainUpperCase(password, nameof(password), UserError.StationPasswordHasNoUpperChars);
            result.StringMustContainNumbers(password, nameof(password), UserError.StationPasswordHasNoDigits);
        }
    }
}