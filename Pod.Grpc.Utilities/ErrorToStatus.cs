#region Licence
/****************************************************************
 *  Filename: ErrorToStatus.cs
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
using Grpc.Core;
using Pod.Data.Infrastructure;
using Pod.Enums;

namespace Pod.Grpc.Utilities
{
    /// <summary>
    /// Helper class that allows to create from IResult Errors and RpcException
    /// This RpcExceptions can and will be send by Grpc to the client where they can be
    /// converted back to IResults. This allows clear signalling of failed requests
    /// </summary>
    public static class ErrorToException
    {
        /// <summary>
        /// Converts a Result to an Rpc Exception to allow to throw it to the client
        /// </summary>
        /// <param name="result">The Result with Error information</param>
        /// <returns>RpcException</returns>
        public static RpcException ToException(this IResult result)
        {
            var metaData = new Metadata();
            var returnStatusCode = StatusCode.Unknown;
            string statusCodeDetail = "No Details available";
            int errorMsgNumber = 0;

            foreach(var errorType in result)
            {
                //Add all errors to Metadata
                var errorTypeName = Enum.GetName(typeof(UserError), errorType.Key);
                foreach (string message in errorType.Value)
                {
                    errorMsgNumber++;
                    if(errorTypeName != null)metaData.Add($"no{errorMsgNumber.ToString()}.{errorTypeName.ToLowerInvariant()}", message);
                }

                //Try to Identify most important/expressive error
                switch(errorType.Key)
                {
                    case UserError.InternalError:
                        returnStatusCode = StatusCode.Internal;
                        statusCodeDetail = string.Join(Environment.NewLine,errorType.Value);
                        break;
                    case UserError.StationInvalidQrCode:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.StationInvalidControlMode:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.StationInvalidDisplayName:
                    case UserError.StationInvalidUserId:
                    case UserError.StationInvalidStationId:
                        returnStatusCode = StatusCode.InvalidArgument;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.StationNotFound:
                        returnStatusCode = StatusCode.NotFound;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.StationNoValidSubscription:
                    case UserError.StationPasswordTooShort:
                    case UserError.StationPasswordTooLong:
                    case UserError.StationPasswordHasNoDigits:
                    case UserError.StationPasswordHasNoSpecialChars:
                    case UserError.StationPasswordHasNotEnoughUniqueChars:
                    case UserError.StationPasswordHasNoUpperChars:
                    case UserError.StationPasswordHasNoLowerChars:
                    case UserError.OrderInvalidCurrencyCode:
                    case UserError.OrderInvalidPaymentAmount:
                    case UserError.OrderInvalidDuration:
                    case UserError.OrderInvalidExpireDate:
                    case UserError.OrderCustomerPaymentReferenceHasWhiteSpace:
                    case UserError.OrderCustomerPaymentReferenceTooLong:
                        returnStatusCode = StatusCode.InvalidArgument;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.OrderAlreadyPayed:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.OrderAlreadyExpired:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.OrderMaximumActiveAmountReached:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ConnectionInvalidParameter:
                    case UserError.ConnectionInvalidId:
                    case UserError.SessionInvalidTimeChange:
                    case UserError.ShellServerInvalidDisplayName:
                    case UserError.ShellServerInvalidHostAddress:
                    case UserError.ShellServerInvalidPort:
                    case UserError.ShellServerInvalidInternalHostAddress:
                    case UserError.ShellServerInvalidInternalPort:
                    case UserError.ShellServerInvalidHeartbeatValue:
                    case UserError.ShellServerInvalidTimeoutValue:
                        returnStatusCode = StatusCode.InvalidArgument;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ShellClientInvalidStationId:
                    case UserError.ShellClientInvalidPassword:
                        returnStatusCode = StatusCode.Unauthenticated;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ShellClientInvalidDeviceIdentity:
                        returnStatusCode = StatusCode.InvalidArgument;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ShellClientNoServerAvailable:
                        returnStatusCode = StatusCode.ResourceExhausted;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ShellClientServerMismatch:
                    case UserError.ShellClientConnectionIdMismatch:
                    case UserError.ShellClientNetworkStateMismatch:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ShellClientConnectionTimedOut:
                    case UserError.ShellClientOtherConnectionStillAlive:
                    case UserError.ShellClientOtherDeviceIdentityConnected:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    case UserError.ApplicationStationNotFound:
                    case UserError.ApplicationDeviceNotFound:
                    case UserError.ApplicationRootNotFound:
                    case UserError.ApplicationNotFound:
                    case UserError.ApplicationInvalidConnectionId:
                    case UserError.ApplicationInvalidSyncTimestamp:
                        returnStatusCode = StatusCode.FailedPrecondition;
                        statusCodeDetail = string.Join(Environment.NewLine, errorType.Value);
                        break;
                    default:
                        returnStatusCode = StatusCode.Unknown;;
                        break;
                }
            }
            //Set most expressive error and all metaData with all errors
            return new RpcException(new Status(returnStatusCode,statusCodeDetail),metaData);
        }


        public static IResult ToResult(this RpcException rpcException)
        {
            var retval = new Result();
            //Process Errors from MetaData
            foreach(Metadata.Entry entry in rpcException.Trailers)
            {
                var enumVal = entry.Key.Split('.');
                //Get the Error Code
                if(enumVal.Length == 2)
                {
                    if (Enum.TryParse<UserError>(enumVal[1], true, out var enumValue))
                    {
                        retval.Add(entry.Value,enumValue);
                    }
                }
                //Set with Unknown Error Code
                else
                {
                    retval.Add(entry.Value, UserError.InternalError);
                }
            }

            if(!retval.HasError())
            {
                //Need to add the general Error
                retval.Add(rpcException.Message, UserError.InternalError);
            }
            //Check for Error from Status Code
            return retval;
        }
    }
}
