#region Licence
/****************************************************************
 *  Filename: Station.cs
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
using System.Security.Cryptography;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Billing;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Users;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// It the root for an Station a.k.a Shell Client
    /// </summary>
    public class Station
    {

        public const int MaximumLengthPaymentReference = 128;

        private HashSet<ClosedConnection> _connectionHistory;
        private HashSet<Session> _sessions;
        private HashSet<ApplicationRoot> _applicationRoots;
        private HashSet<StationApiKey> _apiKeys;

        private Station() { }
        private Station(Guid userId, string passwordHash)
        {
            CreatedOnUtc = DateTime.UtcNow;
            PasswordHash = passwordHash;
            ApplicationUserId = userId;
        }

        public Guid Id { get; private set; }
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// Hash of the Station Password
        /// </summary>
        private string PasswordHash { get; set; }

        /// <summary>
        /// All Application Routes for this Station/ one for each DeviceId
        /// </summary>
        public IReadOnlyCollection<ApplicationRoot> ApplicationRoots => _applicationRoots;

        /// <summary>
        /// General Station Operation Settings
        /// </summary>
        public StationSettings StationSettings { get; private set; }

        /// <summary>
        /// Subscription information for that station
        /// </summary>
        public SubscriptionState SubscriptionState { get; private set; }
        
        /// <summary>
        /// Connection State for this Station
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }
        
        /// <summary>
        /// Session State information for this Station
        /// </summary>
        public SessionDetails SessionDetails { get; private set; }

        /// <summary>
        /// The UserId this instance belongs to
        /// </summary>
        public Guid ApplicationUserId { get; private set; }

        /// <summary>
        /// The Navigational Property for the User
        /// </summary>
        public ApplicationUser ApplicationUser { get; private set; }

        /// <summary>
        /// Collection of ApiKeys for this Station
        /// </summary>
        public IReadOnlyCollection<StationApiKey> ApiKeys => _apiKeys;

        /// <summary>
        /// Connection History for this Station
        /// </summary>
        public IReadOnlyCollection<ClosedConnection> ConnectionHistory => _connectionHistory;

        /// <summary>
        /// Session History for this Station
        /// </summary>
        public IReadOnlyCollection<Session> Sessions => _sessions;

        /// <summary>
        /// Creates an Order for an subscription that can then be purchased 
        /// </summary>
        /// <param name="expiresUtc">The Time the order expires</param>
        /// <param name="timeOrdered">The DateTime the order is created</param>
        /// <param name="paymentAmount">The amount to pay to fulfill the order</param>
        /// <param name="currencyCode">The currency of the payment amount</param>
        /// <param name="sourceIpAddress">The IPAddress the order was created from</param>
        /// <param name="customerOrderReference">A reference provided by the customer for this order</param>
        /// <returns>result</returns>
        public IResult<SubscriptionOrder> CreateOrder(
                DateTime expiresUtc,
                TimeSpan timeOrdered,
                decimal paymentAmount,
                CurrencyIsoCode currencyCode,
                string sourceIpAddress,
                string customerOrderReference = null)
        {
            var retval = new Result<SubscriptionOrder>();
            var created = DateTime.UtcNow;
            //Has SubscriptionState
            retval.RefNotNull(SubscriptionState, nameof(SubscriptionState));

            //Expires is before created
            retval.ArgNotBefore(
                    expiresUtc,
                    nameof(expiresUtc),
                    created,
                    nameof(created),
                    UserError.OrderInvalidExpireDate);

            //Has valid amount of days
            retval.ArgNotLowerOrEqualThen(
                    (TimeSpan)timeOrdered,
                    nameof(timeOrdered),
                    TimeSpan.Zero,
                    "minimum time",
                    UserError.OrderInvalidDuration);

            //Valid Payment Amount
            retval.ArgNotLowerThen(
                    paymentAmount,
                    nameof(paymentAmount),
                    0,
                    "minimum payment",
                    UserError.OrderInvalidPaymentAmount);


            //Valid Currency Code
            retval.ArgNotEnum(
                    typeof(CurrencyIsoCode),
                    currencyCode,
                    CurrencyIsoCode.Unknown,
                    nameof(currencyCode),
                    UserError.OrderInvalidCurrencyCode);

            //Has Payment Reference
            if(customerOrderReference != null)
            {
                retval.StringNotLongerThen(
                        customerOrderReference,
                        nameof(customerOrderReference),
                        MaximumLengthPaymentReference,
                        UserError.OrderCustomerPaymentReferenceTooLong);
            }

            //Has SourceIpAddress
            retval.ArgNotNullOrWhitespace(sourceIpAddress, nameof(sourceIpAddress));

            if(retval.HasError()) return retval;

            return retval.Add(
                    new SubscriptionOrder(
                            ApplicationUserId,
                            SubscriptionState,
                            expiresUtc,
                            timeOrdered,
                            paymentAmount,
                            currencyCode,
                            customerOrderReference,
                            sourceIpAddress));
        }

        /// <summary>
        /// Sets a Password for the Station that the client uses to authenticate
        /// </summary>
        /// <param name="password">The password</param>
        /// <param name="passwordHasher">The hasher the password</param>
        /// <returns>result</returns>
        public IResult SetPassword(string password, IPasswordHasher passwordHasher)
        {
            var retval = new Result();
            retval.ArgNotNullOrWhitespace(password, nameof(password));
            if(retval.IsSuccess()) PasswordHash = passwordHasher.HashPassword(password);
            return retval;
        }

        /// <summary>
        /// Verifies a Password
        /// </summary>
        /// <param name="password">The password</param>
        /// <param name="passwordHasher">The hasher to use for validation</param>
        /// <returns>true if valid, otherwise false</returns>
        public bool VerifyPassword(string password, IPasswordHasher passwordHasher)
        {
            return passwordHasher.VerifyHashedPassword(PasswordHash,password)== PasswordVerificationResult.Success;
        }

        /// <summary>
        /// Creates an ApiKey/Secret for an station
        /// </summary>
        /// <param name="displayName">The name of this key</param>
        /// <returns>result</returns>
        public IResult<StationApiKey> CreateStationApiKey(string displayName)
        {
            var retval = new Result<StationApiKey>();

            //The Station must be first persisted
            retval.ArgNotEqual(Id, nameof(Id), Guid.Empty, UserError.StationInvalidStationId);

            //The Api Keys must be included
            retval.ArgNotNull(_apiKeys,nameof(ApiKeys));
            if(retval.HasError()) return retval;

            //Generate Key and set to keys
            var newKeyResult = StationApiKey.Generate(this, displayName);
            if(retval.Add(newKeyResult).HasError()) return retval;
            _apiKeys.Add(newKeyResult.ReturnValue);

            //Return Key
            return newKeyResult;
        }

        /// <summary>
        /// Removes a ApiKey/Secret from the station
        /// </summary>
        /// <param name="key">The instance to remove</param>
        /// <returns>result</returns>
        public IResult RemoveStationApiKey(StationApiKey key)
        {
            var result = new Result();
            //Reference must be included
            if (!result.ArgNotNull(_apiKeys, nameof(ApiKeys))) return result;
            result.ArgTrue(_apiKeys.Remove(key),"RemoveResult");
            return result;
        }

        /// <summary>
        /// Creates a new Station
        /// </summary>
        /// <param name="userId">The user that this station should belong to</param>
        /// <param name="displayName">The display name of the station</param>
        /// <param name="password">The password for the station</param>
        /// <param name="passwordHasher">The password hasher to use to hash the password</param>
        /// <returns>result</returns>
        public static IResult<Station> Create(Guid userId, string displayName, string password, IPasswordHasher passwordHasher)
        {
            var retval = new Result<Station>();
            retval.ArgNotEmpty(userId, nameof(userId), UserError.StationInvalidUserId);
            retval.ArgNotNullOrWhitespace(displayName, nameof(displayName), UserError.StationInvalidDisplayName);
            retval.ArgNotNullOrWhitespace(password, nameof(password));
            if(retval.HasError()) return retval;

            var newStation = new Station(userId, passwordHasher.HashPassword(password));
            newStation.StationSettings = new StationSettings(newStation, displayName);
            newStation.SubscriptionState = new SubscriptionState(newStation);
            newStation.ConnectionState = new ConnectionState(newStation);
            newStation.SessionDetails = new SessionDetails(newStation);
            return retval.Add(newStation);
        }

        /// <summary>
        /// Helper for EF entity configuration to provide strongly typed naming of private property
        /// </summary>
        /// <returns>Strongly typed name of private PasswordHash Property</returns>
        public static string NameOfPasswordHash() => nameof(PasswordHash);
    }

    /// <summary>
    /// Settings of an Station
    /// </summary>
    public class StationSettings
    {
        private StationSettings() { }
        internal StationSettings(Station station, string displayName)
        {
            DisplayName = displayName;
            ControlMode = StationControlMode.Local;
            Station = station;
        }
        public Guid Id { get; private set; }

        /// <summary>
        /// The name to display for this station
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The QrCode to display when set to an <see cref="StationControlMode"/> with QRCode
        /// </summary>
        public string QRCode { get; private set; }

        /// <summary>
        /// The Mode of the Station
        /// </summary>
        public StationControlMode ControlMode { get; private set; }

        /// <summary>
        /// The Id of the Station this settings belong to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// The Navigation Property for the Station
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// Allows to set the display name
        /// </summary>
        /// <param name="displayName">Name to set</param>
        /// <returns>result</returns>
        public IResult SetDisplayName(string displayName)
        {
            var retval = new Result();
            retval.ArgNotNullOrWhitespace(displayName, nameof(displayName), UserError.StationInvalidDisplayName);
            if(retval.IsSuccess()) DisplayName = displayName;
            return retval;
        }

        /// <summary>
        /// Sets the Control Mode
        /// </summary>
        /// <param name="mode">Mode to set</param>
        /// <returns>result</returns>
        public IResult SetControlMode(StationControlMode mode)
        {
            var retval = new Result();
            retval.ArgNotEnum(
                    typeof(StationControlMode),
                    mode,
                    StationControlMode.Undefined,
                    nameof(mode),
                    UserError.StationInvalidControlMode);
            if(mode == StationControlMode.RemoteWithQrCode && string.IsNullOrWhiteSpace(QRCode))
            {
                retval.InvalidOperation(mode, nameof(mode), "The QRCode is not set", UserError.StationInvalidQrCode);
            }

            if(retval.HasError()) return retval;
            ControlMode = mode;
            return retval;
        }

        /// <summary>
        /// Sets the QrCode
        /// </summary>
        /// <param name="qrCode">QrCode to set</param>
        /// <returns>result</returns>
        public IResult SetQrCode(string qrCode)
        {
            var retval = new Result();
            if(ControlMode == StationControlMode.RemoteWithQrCode && string.IsNullOrWhiteSpace(qrCode))
            {
                retval.InvalidOperation(
                        qrCode,
                        nameof(qrCode),
                        "The qr code can not be cleared when the station mode requires it",
                        UserError.StationInvalidQrCode);
            }

            if(retval.HasError()) return retval;
            QRCode = qrCode;
            return retval;
        }

        /// <summary>
        /// Sets the QrCode and Control Mode in one step
        /// Its useful when the QrCode was previously null and invalid
        /// </summary>
        /// <param name="qrCode">QrCode to use</param>
        /// <param name="mode">The Mode to set to</param>
        /// <returns></returns>
        public IResult SetQrCodeAndControlMode(string qrCode, StationControlMode mode)
        {
            var retval = new Result();
            retval.ArgNotEnum(
                    typeof(StationControlMode),
                    mode,
                    StationControlMode.Undefined,
                    nameof(mode),
                    UserError.StationInvalidControlMode);
            if(mode == StationControlMode.RemoteWithQrCode)
            {
                retval.ArgNotNullOrWhitespace(qrCode, nameof(qrCode), UserError.StationInvalidQrCode);
            }

            if(retval.HasError()) return retval;
            QRCode = qrCode;
            ControlMode = mode;
            return retval;
        }
    }

    /// <summary>
    /// Aggregation Root for Application related to a Station
    /// </summary>
    public class ApplicationRoot
    {
        //cache only for GetLocalApps()
        private Dictionary<Guid, LocalApp> _localAppsDict;

        private HashSet<LocalApp> _localApps;

        private ApplicationRoot() { }
        private ApplicationRoot(Station station, DeviceIdentity deviceIdentity)
        {
            Station = station;
            DeviceIdentity = deviceIdentity;
            LastSyncTimestampUtc = DateTime.UtcNow;
        }
        public Guid Id { get; private set; }

        /// <summary>
        /// Sever side DateTime a full sync cycle was done with the client
        /// </summary>
        public DateTime LastSyncTimestampUtc { get; private set; }
        /// <summary>
        /// Station Id this instance belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// Navigation Property for the station
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// The DeviceId this instance belongs to
        /// This allows one ApplicationRoot per Station Account / Hardware Device combo  
        /// </summary>
        public string DeviceIdentityId { get; set; }

        /// <summary>
        /// The Navigation Property for the station device
        /// </summary>
        public DeviceIdentity DeviceIdentity { get; set; }

        /// <summary>
        /// The related known Apps for this station/device combo 
        /// </summary>
        public IReadOnlyCollection<LocalApp> LocalApps => _localApps;

        /// <summary>
        /// Sets the Synchronization Timestamp that should be done after
        /// every successful full sync cycle 
        /// </summary>
        public void SetLastSyncTimestamp()
        {
            LastSyncTimestampUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new Application Root for a Station/Device combo
        /// </summary>
        /// <param name="station">The Station</param>
        /// <param name="deviceIdentity">The Device</param>
        /// <returns>result</returns>
        public static IResult<ApplicationRoot> CreateApplicationRoot(Station station, DeviceIdentity deviceIdentity)
        {
            var result = new Result<ApplicationRoot>();
            result.ArgNotNull(station, nameof(station));
            result.ArgNotNull(deviceIdentity, nameof(deviceIdentity));
            if(result.HasError()) return result;
            return result.Add(new ApplicationRoot(station, deviceIdentity));
        }

        /// <summary>
        /// Get all known LocalApps as Dictionary
        /// </summary>
        /// <returns>The apps as dict</returns>
        private Dictionary<Guid, LocalApp> GetLocalApps()
        {
            if(_localAppsDict == null)
            {
                _localAppsDict = new Dictionary<Guid, LocalApp>();
                foreach(LocalApp app in _localApps)
                {
                    _localAppsDict.Add(app.UniqueAppId, app);
                }
            }

            return _localAppsDict;
        }

        /// <summary>
        /// Adds a new Local App to the ApplicationRoot
        /// </summary>
        /// <param name="app">The unique app related to the local app</param>
        /// <param name="appUpdate">The AppUpdate providing info about the local app</param>
        /// <returns>result</returns>
        private IResult<bool> AddApp(UniqueApp app, IAppUpdate appUpdate)
        {
            var result = new Result<bool>();
            var createResult = LocalApp.CreateLocalApp(this, app, appUpdate);
            if(createResult.IsSuccess())
            {
                _localApps.Add(createResult.ReturnValue);
                _localAppsDict.Add(createResult.ReturnValue.UniqueAppId, createResult.ReturnValue);
                result.Add(true);
            }
            else
            {
                result.Add(createResult);
            }

            return result;
        }
    }

    /// <summary>
    /// An generic update for an Application that is on an Shell Client
    /// </summary>
    public class LocalAppUpdate
    {
        internal LocalAppUpdate(Guid applicationId, uint instanceVersion)
        {
            InstanceVersion = instanceVersion;
            ApplicationId = applicationId;
        }
        public Guid ApplicationId { get; }
        public uint InstanceVersion { get; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Checks if the data provided in this instance is valid
        /// </summary>
        /// <returns>true if valid</returns>
        internal bool IsValid()
        {
            if(InstanceVersion == 0 ||
               String.IsNullOrWhiteSpace(DisplayName) ||
               ApplicationId == Guid.Empty) return false;
            return true;
        }

        /// <summary>
        /// Compares if the this instance has an higher instance Version 
        /// </summary>
        /// <param name="currentInstanceVersion">The instance version to compare to</param>
        /// <returns>true if this instance has an higher version</returns>
        internal bool IsNewer(uint currentInstanceVersion) { return InstanceVersion > currentInstanceVersion; }

        /// <summary>
        /// Creates an LocalAppUpdate from an IAppUpdate
        /// </summary>
        /// <param name="appUpdate">The IAppUpdate</param>
        /// <returns>result</returns>
        public static LocalAppUpdate FromIAppUpdateInfo(IAppUpdate appUpdate)
        {
            return new LocalAppUpdate(appUpdate.ApplicationId, appUpdate.InstanceVersion)
                   {
                           DisplayName = appUpdate.DisplayName,
                           IsEnabled = appUpdate.IsEnabled,
                   };
        }
    }

    /// <summary>
    /// An ApiKey/Secret for an Station
    /// </summary>
    public class StationApiKey
    {
        private StationApiKey() { }

        private StationApiKey(Station station, string secretKey, string displayName)
        {
            DisplayName = displayName;
            CreatedOnUtc = DateTime.UtcNow;
            Station = station;
            StationId = station.Id;
            SecretKey = secretKey;
        }
        /// <summary>
        /// The Date time when an entity was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// This will is the Id of the ApiKey
        /// </summary>
        public Guid PublicKey { get; private set; }

        /// <summary>
        /// This is the Secret of the Api Key that should not be shared
        /// </summary>
        public string SecretKey { get; private set; }

        /// <summary>
        /// A name that helps to identify the API Key or what he was created for
        /// </summary>
        public string DisplayName { get; private set; }
        /// <summary>
        /// The Station Id the ApiKey belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// The Station the ApiKey belongs to 
        /// </summary>
        public Station Station { get; private set; }

        public static IResult<StationApiKey> Generate(Station station, string displayName)
        {
            var result = new Result<StationApiKey>();
            if(!result.ArgNotNull(station, nameof(station))) return result;

            //Generate a new Secret
            var key = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(key);

            //Store as Base64
            string apiKey = Convert.ToBase64String(key);

            //Set return value
            return result.Add(new StationApiKey(station, apiKey, displayName));
        }
    }
}