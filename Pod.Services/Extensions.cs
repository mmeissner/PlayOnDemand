#region Licence
/****************************************************************
 *  Filename: Extensions.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellHost;
using SessionDetails = Pod.Data.Models.Shell.SessionDetails;
using SessionState = Pod.Grpc.Messages.Shared.SessionState;

namespace Pod.Services
{
    /// <summary>
    /// Collection of Extensions used by Services
    /// </summary>
    public static class Extensions
    {
        public static async Task<Result<T>> VerifyCredentials<T>(
                this ClientCredentials credentials, PodDbContext context)
        {
            var result = new Result<T>();
            return result.Add(await credentials.VerifyCredentials(context));
        }

        /// <summary>
        /// Verifies the ClientCredentials
        /// </summary>
        /// <param name="credentials">The Client Credentials</param>
        /// <param name="context">The DbContext to use</param>
        /// <returns>The Verification Result</returns>
        public static async Task<Result> VerifyCredentials(this ClientCredentials credentials, PodDbContext context)
        {
            //Create a new Result
            var result = new Result();

            //Evaluate if Inputs are Valid
            result.ArgNotEqual(
                    credentials.StationId,
                    nameof(credentials.StationId),
                    Guid.Empty,
                    UserError.ShellClientInvalidStationId);
            result.ArgNotNullOrWhitespace(
                    credentials.Password,
                    nameof(credentials.Password),
                    UserError.ShellClientInvalidPassword);

            //Return Error on Invalid Inputs
            if(result.HasError()) return result;

            //Get the Station by Id
            var stationDb = await context.Stations.FindAsync(credentials.StationId);

            //Return if Station was not found
            result.RefNotNull(stationDb, nameof(stationDb), UserError.ShellClientInvalidStationId);
            if(result.HasError()) return result;

            //Verify provided Password
            result.ValueTrue(
                    stationDb.VerifyPassword(credentials.Password, new PasswordHasher()),
                    "IsPasswordValid",
                    UserError.ShellClientInvalidPassword);
            return result;
        }

        /// <summary>
        /// Verifies if the Connection Id is set
        /// </summary>
        /// <param name="guidAsBytes">The Data Transfer Object for a Guid</param>
        /// <param name="connectionId">The Connection Id</param>
        /// <returns>true if there was a Connection Id set, false if not</returns>
        public static bool HasConnectionId(this GuidAsBytes guidAsBytes, out Guid connectionId)
        {
            var guid = guidAsBytes.ToGuidNullable();
            if(!guid.HasValue || Guid.Empty == guid.Value)
            {
                connectionId = Guid.Empty;
                return false;
            }

            connectionId = guid.Value;
            return true;
        }

        /// <summary>
        /// Converts a Data Transfer Enum to a local Data Model Enum
        /// </summary>
        /// <param name="controlMode">The DT Enum</param>
        /// <returns>The Data Model Enum</returns>
        public static ControlMode ToGrpcControlMode(this StationControlMode controlMode)
        {
            switch(controlMode)
            {
                case StationControlMode.Undefined:
                    return ControlMode.Unset;
                case StationControlMode.Local:
                    return ControlMode.Local;
                case StationControlMode.Remote:
                    return ControlMode.Remote;
                case StationControlMode.RemoteWithQrCode:
                    return ControlMode.RemoteWithQrCode;
                default:
                    throw new ArgumentOutOfRangeException(nameof(controlMode), controlMode, null);
            }
        }

        /// <summary>
        /// Adds Error Information to an Result for an unsuccessful ConnectionRequestResult
        /// </summary>
        /// <param name="result">The Result object to add error information to</param>
        /// <param name="requestResult">The Connection Result</param>
        /// <returns>true if there are no errors, false if there is an error result</returns>
        public static bool IsConnectionResponseSuccess<T>(this Result<T> result, ConnectionRequestResult requestResult)
        {
            return ((Result)result).IsConnectionResponseSuccess(requestResult);
        }

        /// <summary>
        /// Adds Error Information to an Result for an unsuccessful ConnectionRequestResult
        /// </summary>
        /// <param name="result">The Result object to add error information to</param>
        /// <param name="requestResult">The Connection Result</param>
        /// <returns>true if there are no errors, false if there is an error result</returns>
        public static bool IsConnectionResponseSuccess(this Result result, ConnectionRequestResult requestResult)
        {
            switch(requestResult)
            {
                case ConnectionRequestResult.Success:
                    return true;
                case ConnectionRequestResult.ServerMismatch:
                    result.Add("Server Mismatch, was the wrong server contacted?", UserError.ShellClientServerMismatch);
                    break;
                case ConnectionRequestResult.ConnectionIdMismatch:
                    result.Add("Connection Id Mismatch", UserError.ShellClientConnectionIdMismatch);
                    break;
                case ConnectionRequestResult.NetworkStateMismatch:
                    result.Add("Network state Mismatch", UserError.ShellClientNetworkStateMismatch);
                    break;
                case ConnectionRequestResult.ConnectionTimedOut:
                    result.Add("Connection already TimedOut!", UserError.ShellClientConnectionTimedOut);
                    break;
                case ConnectionRequestResult.ConnectionStillAlive:
                    result.Add(
                            "The operation was not possible because a connection is still alive",
                            UserError.ShellClientOtherConnectionStillAlive);
                    break;
                case ConnectionRequestResult.OtherDeviceIdentityConnected:
                    result.Add(
                            "The operation was not possible because a connection with another device id is still in use",
                            UserError.ShellClientOtherConnectionStillAlive);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null);
            }

            return false;
        }

        /// <summary>
        /// Adds Error Information to an Result for an unsuccessful SessionResponse
        /// </summary>
        /// <param name="result">The Result object to add error information to</param>
        /// <param name="sessionResponse">The Session Response</param>
        /// <returns>true if there are no errors, false if there is an error result</returns>
        public static bool IsSessionResponseSuccess<T>(this Result<T> result, SessionResponse sessionResponse)
        {
            return ((Result)result).IsSessionResponseSuccess(sessionResponse);
        }

        /// <summary>
        /// Adds Error Information to an Result for an unsuccessful SessionResponse
        /// </summary>
        /// <param name="result">The Result object to add error information to</param>
        /// <param name="sessionResponse">The Session Response</param>
        /// <returns>true if there are no errors, false if there is an error result</returns>
        public static bool IsSessionResponseSuccess(this Result result, SessionResponse sessionResponse)
        {
            switch(sessionResponse)
            {
                case SessionResponse.Undefined:
                    result.Add("Session Response Value is not set!", UserError.InternalError);
                    break;
                case SessionResponse.Success:
                    return true;
                case SessionResponse.StateMismatch:
                    result.Add(
                            "Session State PreRequirement is not meet!",
                            UserError.ShellClientSessionStatePreRequirementNotMeet);
                    break;
                case SessionResponse.ConnectionMismatch:
                    result.Add("Connection Id Mismatch", UserError.ShellClientConnectionIdMismatch);
                    break;
                case SessionResponse.Timeout:
                    result.Add(
                            "TimedOut already occured for requested Operation!",
                            UserError.ShellClientGetLoginRequestTimeout);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sessionResponse), sessionResponse, null);
            }

            return false;
        }

        /// <summary>
        /// Converts an Data Model Enum to an Data Transfer SessionState
        /// </summary>
        /// <param name="state">The Data Model Enum</param>
        /// <returns>The DTO Enum</returns>
        public static SessionState ToGrpcSessionState(this Pod.Enums.SessionState state)
        {
            switch(state)
            {
                case Enums.SessionState.Requested:
                    return SessionState.LoginRequested;
                case Enums.SessionState.Delivered:
                    return SessionState.AwaitingConfirmation;
                case Enums.SessionState.Started:
                    return SessionState.Running;
                case Enums.SessionState.Ended:
                    return SessionState.NoSession;
                case Enums.SessionState.Canceled:
                    return SessionState.NoSession;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /// <summary>
        /// Converts a Stations Logout Reason to a Data Model Stop Reason
        /// </summary>
        /// <param name="reason">The stations Logout Reason</param>
        /// <returns>The Data Models StopReason</returns>
        public static StopReason ToStopReason(this LogoutReason reason)
        {
            switch(reason)
            {
                case LogoutReason.Unset:
                    return StopReason.Unknown;
                    ;
                case LogoutReason.UserLogout:
                    return StopReason.UserLogout;
                case LogoutReason.Inactivity:
                    return StopReason.Inactivity;
                case LogoutReason.Shutdown:
                    return StopReason.StationShutdown;
                case LogoutReason.LimitReached:
                    return StopReason.LimitReached;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }

        /// <summary>
        /// Checks an SessionDetails and returns the appropriate Value for the ServerDeadlineUtcForResponse  
        /// </summary>
        /// <param name="sessionDetails">The SessionDetails to evaluate</param>
        /// <returns>The resulting Dto</returns>
        public static DateTimeUtcAsLong GetServerDeadlineUtcForResponse(this SessionDetails sessionDetails)
        {
            if(sessionDetails.Session?.SendOnUtc != null)
            {
                return sessionDetails.Session.SendOnUtc.Value.
                                      Add(sessionDetails.TimeoutLoginRequestResponse).
                                      ToDateTimeUtcAsLong();
            }

            return new DateTimeUtcAsLong() {HasValue = false};
        }

        /// <summary>
        /// Checks an SessionDetails and returns the appropriate Value for the ServerDeadlineUtcForPickup  
        /// </summary>
        /// <param name="sessionDetails">The SessionDetails to evaluate</param>
        /// <returns>The resulting Dto</returns>
        public static DateTimeUtcAsLong GetServerDeadlineUtcForPickup(this SessionDetails sessionDetails)
        {
            if(sessionDetails.Session?.RequestedOnUtc != null)
            {
                return sessionDetails.Session.RequestedOnUtc.Add(sessionDetails.TimeoutLoginRequestDelivery).
                                      ToDateTimeUtcAsLong();
            }

            return new DateTimeUtcAsLong() {HasValue = false};
        }

        /// <summary>
        /// Converts an local Data Model SessionDetails to an DTO
        /// </summary>
        /// <param name="sessionDetails">The Data Model SessionDetails</param>
        /// <returns>The Dto</returns>
        public static Pod.Grpc.Messages.ShellHost.SessionDetails ToGrpcSessionDetails(
                this SessionDetails sessionDetails)
        {
            return new Pod.Grpc.Messages.ShellHost.SessionDetails()
                   {
                           SessionId = sessionDetails.Session.Id.ToGuidAsBytes(),
                           RequestedOnUtc = sessionDetails.Session.RequestedOnUtc.ToDateTimeUtcAsLong(),
                           SessionState = sessionDetails.Session.State.ToGrpcSessionState(),
                           DeadlineUtcForPickUp = sessionDetails.GetServerDeadlineUtcForPickup(),
                           DeadlineUtcForConfirmation = sessionDetails.GetServerDeadlineUtcForResponse(),
                           MaxTimeForConfirmationDecision = sessionDetails.UserTimeForLoginRequestResponse.ToTimeSpanAsLong(),
                           EffectiveDuration = sessionDetails.Session.Duration.ToTimeSpanAsLong(),
                           StartTimeUtc = sessionDetails.Session.StartedUtc.ToDateTimeUtcAsLong(),
                           Conditions = sessionDetails.Session.SessionRule.ToGrpcSessionConditions()
                   };
        }

        /// <summary>
        /// Converts the <see cref="SessionDetails"/> to an Grpc response picking up an intended session requiring confirmation.
        /// </summary>
        /// <param name="sessionDetails">The session details.</param>
        /// <returns>Info about the intended/requested Session and timeouts for it confirmation</returns>
        public static RequestedLoginResponse ToGrpcRequestedLoginResponse(this SessionDetails sessionDetails)
        {
            return new RequestedLoginResponse()
                   {
                           SessionDetails = sessionDetails.ToGrpcSessionDetails(),
                   };
        }

        /// <summary>
        /// Converts a Data Model SessionRule to the related Dto
        /// </summary>
        /// <param name="rule">The Data Model SessionRule</param>
        /// <returns>The related Dto</returns>
        public static SessionConditions ToGrpcSessionConditions(this SessionRule rule)
        {
            if(rule == null) return null;

            var retval = new SessionConditions()
                   {
                           InitialDurationOnSessionStart = rule.StartDuration.ToTimeSpanAsLong(),
                           AutostartAppIdOnSessionStart = rule.StartApplication.ToGuidAsBytes(),
                           AutoLogoutOnAppExit = rule.StartApplication.HasValue ? true : false,
                   };
            if(rule.AllowedApps != null)
            {
                foreach(SessionRuleLocalApp app in rule.AllowedApps)
                {
                    retval.AllowedApps.Add(app.LocalApp.UniqueAppId.ToGuidAsBytes());
                }
            }
            return retval;
        }

        /// <summary>
        /// Converts the <see cref="SessionDetails"/> to a Grpc response for an Request for a new LoginIntention.
        /// </summary>
        /// <param name="sessionDetails">The session details.</param>
        /// <returns>Info about the new intended/requested Session and timeouts for its pickup by the client</returns>
        public static LoginRequestResponse ToGrpcLoginRequestResponse(this SessionDetails sessionDetails)
        {
            return new LoginRequestResponse()
                   {
                           SessionDetails = sessionDetails.ToGrpcSessionDetails(),
                   };
        }

        /// <summary>
        /// Detects if an SignInResult has an error and adds it to the Result 
        /// </summary>
        /// <param name="result">The Result to add an error</param>
        /// <param name="signInResult">The SignInResult to evaluate</param>
        /// <returns>The Result</returns>
        public static Result<T> AddSignResult<T>(this Result<T> result, SignInResult signInResult)
        {
            return (Result<T>)((Result)result).AddSignResult(signInResult);
        }

        /// <summary>
        /// Detects if an SignInResult has an error and adds it to the Result 
        /// </summary>
        /// <param name="result">The Result to add an error</param>
        /// <param name="signInResult">The SignInResult to evaluate</param>
        /// <returns>The Result</returns>
        public static Result AddSignResult(this Result result, SignInResult signInResult)
        {
            if(signInResult.Succeeded) return result;
            if(signInResult.IsLockedOut) result.Add("User is locked out!", UserError.UserIdentityIsLockedOut);
            if(signInResult.IsNotAllowed)
                result.Add("User is not allowed to login!", UserError.UserIdentityNotAllowedToLogin);
            // Plain wrong password: SignInResult.Succeeded is false but none of the specific
            // failure flags above are set. Without this branch the caller's HasError() check
            // returned false and AuthenticationService.GetTokenByLogin would mint a token for
            // an unauthenticated request. Treat any non-specific failure as a credential
            // mismatch.
            if(!signInResult.Succeeded && !signInResult.IsLockedOut && !signInResult.IsNotAllowed)
            {
                result.Add("Invalid credentials.", UserError.UserIdentityPasswordMismatch);
            }
            return result;
        }

        /// <summary>
        /// Converts an Identity Result Error to an Error for an Result
        /// </summary>
        /// <param name="result">The Result to add an error to</param>
        /// <param name="identityResult">The Identity Result</param>
        /// <returns>The Result</returns>
        public static Result<T> Add<T>(this Result<T> result, IdentityResult identityResult)
        {
            return (Result<T>)((Result)result).Add(identityResult);
        }
        
        /// <summary>
        /// Converts an Identity Result Error to an Error for an Result
        /// </summary>
        /// <param name="result">The Result to add an error to</param>
        /// <param name="identityResult">The Identity Result</param>
        /// <returns>The Result</returns>
        public static Result Add(this Result result, IdentityResult identityResult)
        {
            if(!identityResult.Succeeded){
                foreach(IdentityError identityError in identityResult.Errors)
                {
                    switch(identityError.Code)
                    {
                        case nameof(IdentityErrorDescriber.DefaultError):
                            return result.Add(identityError.Description, UserError.UserIdentityDefaultError);
                        case nameof(IdentityErrorDescriber.ConcurrencyFailure):
                            return result.Add(identityError.Description, UserError.UserIdentityConcurrencyFailure);
                        case nameof(IdentityErrorDescriber.PasswordMismatch):
                            return result.Add(identityError.Description, UserError.UserIdentityPasswordMismatch);
                        case nameof(IdentityErrorDescriber.InvalidToken):
                            return result.Add(identityError.Description, UserError.UserIdentityInvalidToken);
                        case nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed):
                            return result.Add(
                                    identityError.Description,
                                    UserError.UserIdentityRecoveryCodeRedemptionFailed);
                        case nameof(IdentityErrorDescriber.LoginAlreadyAssociated):
                            return result.Add(identityError.Description, UserError.UserIdentityLoginAlreadyAssociated);
                        case nameof(IdentityErrorDescriber.InvalidUserName):
                            return result.Add(identityError.Description, UserError.UserIdentityInvalidUserName);
                        case nameof(IdentityErrorDescriber.InvalidEmail):
                            return result.Add(identityError.Description, UserError.UserIdentityInvalidEmail);
                        case nameof(IdentityErrorDescriber.DuplicateUserName):
                            return result.Add(identityError.Description, UserError.UserIdentityDuplicateUserName);
                        case nameof(IdentityErrorDescriber.DuplicateEmail):
                            return result.Add(identityError.Description, UserError.UserIdentityDuplicateEmail);
                        case nameof(IdentityErrorDescriber.InvalidRoleName):
                            return result.Add(identityError.Description, UserError.UserIdentityInvalidRoleName);
                        case nameof(IdentityErrorDescriber.DuplicateRoleName):
                            return result.Add(identityError.Description, UserError.UserIdentityDuplicateRoleName);
                        case nameof(IdentityErrorDescriber.UserAlreadyHasPassword):
                            return result.Add(identityError.Description, UserError.UserIdentityUserAlreadyHasPassword);
                        case nameof(IdentityErrorDescriber.UserLockoutNotEnabled):
                            return result.Add(identityError.Description, UserError.UserIdentityUserLockoutNotEnabled);
                        case nameof(IdentityErrorDescriber.UserAlreadyInRole):
                            return result.Add(identityError.Description, UserError.UserIdentityUserAlreadyInRole);
                        case nameof(IdentityErrorDescriber.UserNotInRole):
                            return result.Add(identityError.Description, UserError.UserIdentityUserNotInRole);
                        case nameof(IdentityErrorDescriber.PasswordTooShort):
                            return result.Add(identityError.Description, UserError.UserIdentityPasswordTooShort);
                        case nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars):
                            return result.Add(
                                    identityError.Description,
                                    UserError.UserIdentityPasswordRequiresUniqueChars);
                        case nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric):
                            return result.Add(
                                    identityError.Description,
                                    UserError.UserIdentityPasswordRequiresNonAlphanumeric);
                        case nameof(IdentityErrorDescriber.PasswordRequiresDigit):
                            return result.Add(identityError.Description, UserError.UserIdentityPasswordRequiresDigit);
                        case nameof(IdentityErrorDescriber.PasswordRequiresLower):
                            return result.Add(identityError.Description, UserError.UserIdentityPasswordRequiresLower);
                        case nameof(IdentityErrorDescriber.PasswordRequiresUpper):
                            return result.Add(identityError.Description, UserError.UserIdentityPasswordRequiresUpper);
                        default:
                            return result.Add(identityError.Description, UserError.UserIdentityDefaultError);
                    }
                }
            }
            return result;
        }
    }
}