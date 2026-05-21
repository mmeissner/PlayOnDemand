#region Licence
/****************************************************************
 *  Filename: StationViewModels.cs
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
using System.Linq;
using System.Linq.Expressions;
using Pod.Data.Models.Shell;
using Pod.ViewModels.Customer;
using SubscriptionState = Pod.ViewModels.Customer.SubscriptionState;

namespace Pod.ViewModels.Expressions
{
    public static class ToStationCurrentStateVm
    {
        public static readonly Func<Station, StationCurrentStateViewModel> FuncFromStation = FromStation().Compile();

        public static Expression<Func<Station, StationCurrentStateViewModel>> FromStation()
        {
            // The Session sub-projection is inlined here instead of calling
            // ToSessionViewVm.FuncFromSession(...) so EF Core 10 can translate the whole
            // expression tree to SQL. Calling a compiled delegate from inside an Expression
            // body lands as a constant Func reference in the projection, which EF 10 rejects
            // with the "client projection contains a reference to a constant expression"
            // memory-leak guard (previously a silent client-eval in EF Core 2.1).
            return x => new StationCurrentStateViewModel
                        {
                                StationId = x.Id,
                                DisplayName = x.StationSettings.DisplayName,
                                ControlMode = x.StationSettings.ControlMode,
                                NetworkState = x.ConnectionState.NetworkState,
                                Session = x.SessionDetails.Session != null
                                        ? new SessionViewModel
                                          {
                                                  SessionId = x.SessionDetails.Session.Id,
                                                  Reference = x.SessionDetails.Session.RequestReference,
                                                  State = x.SessionDetails.Session.State,
                                                  StartedOnUtc = x.SessionDetails.Session.StartedUtc,
                                                  StartDuration = x.SessionDetails.Session.SessionRule != null
                                                          ? x.SessionDetails.Session.SessionRule.StartDuration
                                                          : null,
                                                  MaxDurationLimit = x.SessionDetails.Session.Duration,
                                          }
                                        : null,
                        };
        }
    }

    public static class ToStationSettingsVm
    {
        public static readonly Func<Station, StationSettingsViewModel> FuncFromStation = FromStation().Compile();
        public static Expression<Func<Station, StationSettingsViewModel>> FromStation()
        {
            return x => new StationSettingsViewModel
                        {
                                StationId = x.Id,
                                DisplayName = x.StationSettings.DisplayName,
                                QrCode = x.StationSettings.QRCode,
                                ControlMode = x.StationSettings.ControlMode
                        };
        }
    }



    public static class ToSessionLogVm
    {
        /// <summary>
        /// Should be only used for transformation to a view model 
        /// </summary>
        public static readonly Func<Session, SessionLogViewModel> FuncFromSession = FromSession().Compile();
        
        private static Expression<Func<Session, SessionLogViewModel>> FromSession()
        {
            return (x) => new SessionLogViewModel
                          {
                                  StationId = x.StationId,
                                  SessionId = x.Id,
                                  RequestedBy = x.RequestedBy,
                                  LatestState = x.State,
                                  StartedUtc = x.StartedUtc,
                                  Reference = x.RequestReference,
                                  MaxDurationLimit = x.Duration,
                                  EndedUtc = x.StoppedUtc,
                                  StoppedBy = x.StopReason,
                                  //This will lead to sub queries if provided to an DbContext
                                  ChangeRequests = x.ChangeRequests.Any() ? x.ChangeRequests.
                                                                              Select(
                                                                                      ToChangeRequestVm.
                                                                                              FuncFromChangeRequest).
                                                                              OrderBy(y => y.CreatedOnUtc).
                                                                              AsEnumerable() : null
                          };
        }
    }

    public static class ToSessionViewVm
    {
        public static readonly Func<Session,SessionRule, SessionViewModel> FuncFromSession = FromSession().Compile();
        public static Expression<Func<Session, SessionRule, SessionViewModel>> FromSession()
        {
            return (x,y) => x != null ? new SessionViewModel
                                    {
                                            SessionId = x.Id,
                                            Reference = x.RequestReference,
                                            State = x.State,
                                            StartedOnUtc = x.StartedUtc,
                                            StartDuration = y != null ? y.StartDuration : null,
                                            MaxDurationLimit = x.Duration
                                    } : null;
        }
    }


    public static class ToChangeRequestVm
    {
        public static readonly Func<ChangeRequest, ChangeRequestViewModel> FuncFromChangeRequest =
                FromChangeRequest().Compile();
        public static Expression<Func<ChangeRequest, ChangeRequestViewModel>> FromChangeRequest()
        {
            return x => new ChangeRequestViewModel
                        {
                                Id = x.Id,
                                CreatedOnUtc = x.CreatedOnUtc,
                                Reference = x.Reference,
                                TimeChange = x.TimeChange
                        };
        }
    }

    public static class ToConnectionLogVm
    {
        public static Expression<Func<ClosedConnection, StationConnectionLogViewModel>> FromClosedConnection()
        {
            return x => new StationConnectionLogViewModel
                        {
                                Id = x.Id,
                                ServerId = x.ServerId,
                                ConnectionId = x.ConnectionId,
                                DeviceIdentityId = x.DeviceIdentityId,
                                RequestedServerOnUtc = x.RequestedServerOnUtc,
                                ConnectedToServerOnUtc = x.ConnectedToServerOnUtc,
                                DisconnectedOnUtc = x.DisconnectedOnUtc,
                                ClosedBy = x.ClosedBy
                        };
        }
    }

    public static class ToStationApiKeyVm
    {
        /// <summary>
        /// Full projection - includes the SecretKey. Use ONLY at mint time
        /// (StationApiKeyService.CreateStationApiKey). Returning the secret on
        /// subsequent reads would let any operator with list access read every
        /// kiosk's persisted credential.
        /// </summary>
        public static readonly Func<StationApiKey, StationApiKeyViewModel> FuncFromStationApiKey =
                FromStationApiKey().Compile();

        /// <summary>
        /// List projection - secret elided. Use for GET endpoints (the operator UI
        /// shows the key name + public id + creation date; it never re-reads the secret).
        /// </summary>
        public static readonly Func<StationApiKey, StationApiKeyViewModel> FuncFromStationApiKeyNoSecret =
                FromStationApiKeyNoSecret().Compile();

        public static Expression<Func<StationApiKey, StationApiKeyViewModel>> FromStationApiKey()
        {
            return x => new StationApiKeyViewModel
                        {
                                CreateOnUtc = x.CreatedOnUtc,
                                Name = x.DisplayName,
                                PublicKey = x.PublicKey.ToString("N"),
                                Secret = x.SecretKey,
                        };
        }

        public static Expression<Func<StationApiKey, StationApiKeyViewModel>> FromStationApiKeyNoSecret()
        {
            return x => new StationApiKeyViewModel
                        {
                                CreateOnUtc = x.CreatedOnUtc,
                                Name = x.DisplayName,
                                PublicKey = x.PublicKey.ToString("N"),
                                Secret = null,
                        };
        }
    }
}