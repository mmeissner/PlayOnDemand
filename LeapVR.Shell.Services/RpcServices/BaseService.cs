#region Licence
/****************************************************************
 *  Filename: BaseService.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Services.Data;
using NLog;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Base.Client;
using Pod.Grpc.Utilities;

namespace LeapVR.Shell.Services.RpcServices
{
    internal abstract class BaseService :  IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private volatile RpcConnection _connection;
        private readonly Subject<IRpcConnection> _whenConnectionChanges = new Subject<IRpcConnection>();
        private readonly Subject<IServiceErrorInfo> _whenErrorOccures = new Subject<IServiceErrorInfo>();
        protected readonly GrpcConnectionHandler Handler;
        private readonly ConcurrentDictionary<Type, IServiceErrorInfo> _serviceErrors =
                new ConcurrentDictionary<Type, IServiceErrorInfo>();
        protected BaseService(GrpcConnectionHandler connectionHandler)
        {
            TimeSkew = new TimeSpan();
            _connection = new RpcConnection(
                connectionHandler.Settings.HostDetails.ServerHost,
                connectionHandler.Settings.HostDetails.ServerPort);
            Handler = connectionHandler;
            UpdateConnectionState(out _);
        }
        public TimeSpan TimeSkew { get; set; }
        public IEnumerable<IServiceErrorInfo> ServiceErrors => _serviceErrors.Values;
        public IRpcConnection Connection => _connection;
        public IObservable<IRpcConnection> WhenConnectionChanges => _whenConnectionChanges.AsObservable();
        public IObservable<IServiceErrorInfo> WhenErrorOccures => _whenErrorOccures.AsObservable();

        public void SetRpcCallDeadLine(uint ms) { Handler.Settings.RpcTimeoutMs = ms; }
        public abstract Task<IResult> ConnectServiceAsync();
        public void ClearAllErrors([CallerMemberName] string caller = "Unknown")
        {
            Logger.Warn($"{caller} requested to clear all errors");
            _serviceErrors.Clear();
        }

        protected IResult<TT> RpcWrapper<T,TT>(Func<T> rpcCall, Func<T,TT> converter)
        {
            var retval = new Result<TT>();
            StatusCode statusCode = StatusCode.OK;
            try
            {
                var response = rpcCall.Invoke();
                return retval.Add(converter.Invoke(response));
            }
            catch (Exception exception)
            {
                if (exception is RpcException rpcException)
                {
                    Logger.Warn(rpcException, "RpcException during call!");
                    statusCode = rpcException.StatusCode;
                    UpdateServiceErrors(rpcException);
                    return retval.Add(rpcException.ToResult());
                }
                Logger.Error(exception, "Exception during RpcCall");
                return retval.Add(exception.Message, UserError.InternalError);
            }
            finally
            {
                UpdateConnectionAndErrorStateIfRequired(statusCode);
                Logger.Debug("Wrapped RPCCall finished");
            }
        }
        protected IResult<TT> RpcWrapper<T,TT>(Func<T> rpcCall, Func<TimeSpan,T,TT> converter)
        {
            var retval = new Result<TT>();
            StatusCode statusCode = StatusCode.OK;
            try
            {
                var response = rpcCall.Invoke();
                return retval.Add(converter.Invoke(TimeSkew,response));
            }
            catch (Exception exception)
            {
                if (exception is RpcException rpcException)
                {
                    Logger.Warn(rpcException, "RpcException during call!");
                    statusCode = rpcException.StatusCode;
                    UpdateServiceErrors(rpcException);
                    return retval.Add(rpcException.ToResult());
                }
                Logger.Error(exception, "Exception during RpcCall");
                return retval.Add(exception.Message, UserError.InternalError);
            }
            finally
            {
                UpdateConnectionAndErrorStateIfRequired(statusCode);
                Logger.Debug("Wrapped RPCCall finished");
            }
        }
        protected async Task<IResult<TT>> RpcWrapperAsync<T,TT>(Func<Task<T>> rpcCall, Func<TimeSpan,T,TT> converter)
        {
            Logger.Debug("Wrapped RPCCall started");
            var retval = new Result<TT>();
            StatusCode statusCode = StatusCode.OK;
            try
            {
                var response = await rpcCall.Invoke().ConfigureAwait(true);
                return retval.Add(converter.Invoke(TimeSkew,response));
            }
            catch (Exception exception)
            {
                if (exception is RpcException rpcException)
                {
                    Logger.Warn(rpcException, "RpcException during call!");
                    statusCode = rpcException.StatusCode;
                    UpdateServiceErrors(rpcException);
                    return retval.Add(rpcException.ToResult());
                }
                Logger.Error(exception, "Exception during RpcCall");
                return retval.Add(exception.Message, UserError.InternalError);
            }
            finally
            {
                UpdateConnectionAndErrorStateIfRequired(statusCode);
                Logger.Debug("Wrapped RPCCall finished");
            }
        }
        protected async Task<IResult<TT>> RpcWrapperAsync<T,TT>(Func<Task<T>> rpcCall, Func<T,TT> converter)
        {
            Logger.Debug("Wrapped RPCCall started");
            var retval = new Result<TT>();
            StatusCode statusCode = StatusCode.OK;
            try
            {
                var response = await rpcCall.Invoke().ConfigureAwait(true);
                return retval.Add(converter.Invoke(response));
            }
            catch(Exception exception)
            {
                if(exception is RpcException rpcException)
                {
                    Logger.Warn(rpcException, "RpcException during call!");
                    statusCode = rpcException.StatusCode;
                    UpdateServiceErrors(rpcException);
                    return retval.Add(rpcException.ToResult());
                }
                Logger.Error(exception,"Exception during RpcCall");
                return retval.Add(exception.Message,UserError.InternalError);
            }
            finally
            {
                UpdateConnectionAndErrorStateIfRequired(statusCode);
                Logger.Debug("Wrapped RPCCall finished");
            }
        }
        protected void UpdateConnectionAndErrorStateIfRequired(StatusCode code)
        {
            try
            {
                Logger.Info($"UpdateConnectionAndErrorStateIfRequired started with StatusCode={code}");
                bool updateConnectionState = false;

                //Would be on the first connect or after we had a disconnected problem
                if(_connection.State == RpcConnectionStatus.Connecting ||
                   _connection.State == RpcConnectionStatus.Disconnected)
                {
                    updateConnectionState = true;
                }
                else
                {
                    updateConnectionState = IsConnectionCheckRequired(code);
                }

                if(updateConnectionState && UpdateConnectionState(out var newConnection))
                {
                    var result = BeforeConnectionUpdate(newConnection);
                    //Canceled
                    if(result == null) return;
                    _connection = result;
                    PublishConnectionChange(result);
                }

                //Reset an previous Error if it went away
                if(!_serviceErrors.IsEmpty && code == StatusCode.OK)
                {
                    ClearAllErrors();
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
            finally
            {
                Logger.Info($"UpdateConnectionAndErrorStateIfRequired finished");
            }
        }

        /// <summary>
        /// Before the connection gets updated and published.
        /// Allows the Service to cancel publishing or manipulate the result
        /// </summary>
        /// <param name="newStatus">The new status detected.</param>
        /// <returns>The status to be published, null if publishing should be canceled</returns>
        protected virtual RpcConnection BeforeConnectionUpdate(RpcConnection newStatus)
        {
            return newStatus;
        }
        private void UpdateServiceErrors(RpcException rpcException,[CallerMemberName] string caller = "Unknown")
        {
            var errorResult = rpcException.ToResult();
            IServiceErrorInfo retval = null;
            Logger.Error($"Error in Response from {caller} with StatusCode={rpcException.StatusCode}, ErrorMessage = {errorResult.ToErrorString()}");
            //Evaluate for Connection Errors
            switch(rpcException.StatusCode)
            {
                case StatusCode.Unauthenticated:
                    retval = new LicenseError(LicenseStatus.InvalidUsernamePassword, "Invalid Username or Password");
                    //There was an error and we need to add or update
                    _serviceErrors.AddOrUpdate(retval.GetType(), retval, (type, info) => retval);

                    //Inform about Error
                    Logger.Debug($"Publishing RPC Error: {retval.LogJson()}");
                    _whenErrorOccures.OnNext(retval);
                    break;
                case StatusCode.PermissionDenied:
                    retval = new LicenseError(LicenseStatus.LicenseNotFound, "Invalid License State");
                    //There was an error and we need to add or update
                    _serviceErrors.AddOrUpdate(retval.GetType(), retval, (type, info) => retval);

                    //Inform about Error
                    Logger.Debug($"Publishing RPC Error: {retval.LogJson()}");
                    _whenErrorOccures.OnNext(retval);
                    break;
                case StatusCode.DeadlineExceeded:
                case StatusCode.Aborted:
                case StatusCode.Cancelled:
                    break;
                default:
                    retval = new ServiceError(rpcException.StatusCode.ToString());
                    _serviceErrors.AddOrUpdate(retval.GetType(), retval, (type, info) => retval);
                    break;
            }
        }
        private bool IsConnectionCheckRequired(StatusCode code)
        {
            //All Codes above 100 are return values from the server side
            if ((uint)code > 100) return false;
            switch (code)
            {
                case StatusCode.DeadlineExceeded:
                case StatusCode.Unavailable:
                case StatusCode.Unknown:
                    return true;
            }
            return false;
        }
        private bool UpdateConnectionState(out RpcConnection newConnection)
        {
            //See State Changes From/To Matrix
            //https://github.com/grpc/grpc/blob/master/doc/connectivity-semantics-and-api.md
            var channelState = Handler.GrpChannel.State;
            newConnection = null;
            RpcConnectionStatus connectionStatus;
            Logger.Debug(
                $"Evaluate Connection for Host={_connection.Host}, Port={_connection.Port}, ChannelState={channelState}");
            switch (channelState)
            {
                case ChannelState.Idle:
                    connectionStatus = RpcConnectionStatus.Disconnected;
                    break;
                case ChannelState.Connecting:
                case ChannelState.TransientFailure:
                    connectionStatus = RpcConnectionStatus.Connecting;
                    break;
                case ChannelState.Ready:
                    connectionStatus = RpcConnectionStatus.Connected;
                    break;
                case ChannelState.Shutdown:
                    connectionStatus = RpcConnectionStatus.Broken;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (connectionStatus == _connection.State) return false;
            newConnection = _connection.Clone(connectionStatus);
            return true;
        }
        private void PublishConnectionChange(IRpcConnection connection)
        {
            try
            {
                Logger.Debug($"Publishing Connection Change {connection.LogJson()}");
                _whenConnectionChanges.OnNext(connection);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error during publishing of Connection State");
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _whenConnectionChanges?.Dispose();
                _whenErrorOccures?.Dispose();
                Handler?.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
