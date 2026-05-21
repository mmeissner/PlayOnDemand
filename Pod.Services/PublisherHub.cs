#region Licence
/****************************************************************
 *  Filename: PublisherHub.cs
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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Pod.Services
{
    /// <summary>
    /// Allows to publish/send a message to an receiver/client with an unique identifier through a queue in an thread-safe way
    /// </summary>
    /// <typeparam name="TT">The type of the published message</typeparam>
    public class PublisherHub<TT>
    {
        private bool _isAlive = true;
        readonly ConcurrentDictionary<Guid, IMessageHandler> _handlers = new ConcurrentDictionary<Guid, IMessageHandler>();

        /// <summary>
        /// Sends a StopSignal to all Clients and Cancels all notifications after a delay
        /// </summary>
        /// <param name="stopSignal"></param>
        /// <returns>A Task that finishes when all Clients are stopped or stop has timed out</returns>
        public async Task ShutdownAsync(TT stopSignal)
        {
            _isAlive = false;
            var listClients = new List<Guid>(_handlers.Keys);
            int roundsPast = 0;
            do
            {
                foreach (Guid guid in listClients)
                {
                    if (_handlers.TryGetValue(guid, out var publisher))
                    {
                        publisher.AddMessage(stopSignal);
                    }
                }
                roundsPast++;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                listClients = new List<Guid>(_handlers.Keys);
                if (roundsPast > 2)
                {                    
                    //We going to Cancel all and leave
                    listClients = new List<Guid>(_handlers.Keys);
                    foreach(Guid guid in listClients)
                    {
                        if (_handlers.TryGetValue(guid, out var publisher))
                        {
                            publisher.CancelAll();
                        }
                    }
                    listClients.Clear();
                }
            } while(listClients.Any());
        }

        /// <summary>
        /// Publish a message to a client through its notification stream in case the client is connected
        /// </summary>
        /// <param name="stationId">The clients Id</param>
        /// <param name="notification">The message to publish</param>
        /// <returns>true if a handler for the station was found, false if no handler is present</returns>
        public bool Publish(Guid stationId,TT notification)
        {
            if(_handlers.TryGetValue(stationId,out var handler) && _isAlive)
            {
                handler.AddMessage(notification);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Gets an new handler for an receiver and closes any previously existing one for that receiver.
        /// </summary>
        /// <typeparam name="T">The message type for the receiver</typeparam>
        /// <param name="receiverId">The receiver identifier.</param>
        /// <param name="writer">The writer were messages are send through.</param>
        /// <param name="converter">The func to  convert <see cref="TT"/> to <see cref="T"/>.</param>
        /// <param name="funcStopSignalDetector">The func to detect if an <see cref="TT"/> is the signal to stop the delivery Task <see cref="MessageHandler{T}.ReceiveMessages"/>.</param>
        /// <param name="initialItems">The initial items that should be add to the queue from the beginning.</param>
        /// <param name="maxCapacity">The maximum capacity of the queue.</param>
        /// <returns>A message sender for an async method that streams messages, or null when the Hub is considered dead</returns>
        public IMessageSender GetHandler<T>(Guid receiverId, IServerStreamWriter<T> writer, Func<TT, T> converter, Func<TT,bool> funcStopSignalDetector, IEnumerable<TT> initialItems, int maxCapacity = 25)
        {
            if(!_isAlive) return null;
            var handler = new MessageHandler<T>(receiverId,writer, converter,funcStopSignalDetector, RemoveHandler ,initialItems,maxCapacity);
            //Adds a new Handler if none is present for that receiver
            //If an receiver is present then the old handler will receive a cancellation and a new one will be added
            _handlers.AddOrUpdate(receiverId,x => handler,(x, y) =>
                                                                 {
                                                                     if(!y.Equals(handler))y.CancelAll();
                                                                     _handlers[x] = handler;
                                                                     return handler;});
            return handler;
        }


        /// <summary>
        /// Function that Removes the handler />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="receiverId">The receiver identifier.</param>
        /// <param name="messageHandler">The message handler.</param>
        private void RemoveHandler<T>(Guid receiverId, MessageHandler<T> messageHandler)
        {
            ((ICollection<KeyValuePair<Guid, IMessageHandler>>)_handlers).Remove(
                    new KeyValuePair<Guid, IMessageHandler>(receiverId, messageHandler));
        }

        /// <summary>
        /// Handles delivery of published messages to an receiver
        /// </summary>
        /// <typeparam name="T">The message type to send</typeparam>
        private class MessageHandler<T> : IMessageHandler
        {
            private readonly IServerStreamWriter<T> _writer;
            private readonly Func<TT, T> _converter;
            private readonly Func<TT, bool> _funcStopSignalDetector;
            private readonly Action<Guid,MessageHandler<T>> _funcRemoveHandler;
            readonly BlockingCollection<TT> _messages;
            readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly Guid _receiverId;

            /// <summary>
            /// The <see cref="MessageHandler{T}"/> allows to send queued messages through <see cref="IServerStreamWriter{T}"/>
            /// Whenever a new message is queued it will be send by <see cref="ReceiveMessages"/>
            /// </summary>
            /// <param name="receiverId">The identifier of the receiver.</param>
            /// <param name="writer">The instance were <see cref="T"/> is send to.</param>
            /// <param name="converter">The converter to transform a published message <see cref="TT"/> to <see cref="T"/></param>
            /// <param name="funcStopSignalDetector">A function that evaluates <see cref="TT"/> for an stop condition for <see cref="ReceiveMessages"/>.</param>
            /// <param name="funcRemoveHandler">The function that removes the handler when <see cref="ReceiveMessages"/> finishes</param>
            /// <param name="initialItems">Items for the queue that should be there from the beginning</param>
            /// <param name="maxCapacity">The maximum capacity of the queue.</param>
            public MessageHandler(Guid receiverId, IServerStreamWriter<T> writer, Func<TT, T> converter,Func<TT,bool> funcStopSignalDetector,Action<Guid,MessageHandler<T>> funcRemoveHandler,IEnumerable<TT> initialItems, int maxCapacity)
            {
                _messages = new BlockingCollection<TT>(new ConcurrentQueue<TT>(initialItems),maxCapacity);
                _receiverId = receiverId;
                _writer = writer;
                _converter = converter;
                _funcStopSignalDetector = funcStopSignalDetector;
                _funcRemoveHandler = funcRemoveHandler;
                Created = DateTime.UtcNow;
               
            }
            
            /// <summary>
            /// Returns if the Handler was canceled
            /// </summary>
            public bool IsCanceled => _cts.IsCancellationRequested;
            
            /// <summary>
            /// The DateTime the Handler was created
            /// </summary>
            public DateTime Created { get; }
            
            /// <summary>
            /// Triggers an Cancellation for that Message Handler
            /// </summary>
            public void CancelAll()
            {
                _cts.Cancel();
            }
            
            /// <summary>
            /// Adds a message to send to the queue of the Handler 
            /// </summary>
            /// <param name="message">The message to send</param>
            public void AddMessage(TT message)
            {
                _messages.Add(message,_cts.Token);
            }

            /// <summary>
            /// Gets the messages in the queue and sends them over the <see cref="IServerStreamWriter{T}"/>.
            /// Invokes the removal function when the Receive Tasks ends
            /// </summary>
            /// <param name="cancellation">The cancellation.</param>
            /// <returns>A Tasks that ends when there are no more messages to receive</returns>
            public async Task ReceiveMessages(CancellationToken cancellation)
            {   
                try
                {
                    bool disconnectReceived = false;
                    do
                    {
                        //var message = _messages.Take(_cts.Token);
                        if(_messages.TryTake(out var message, 5000,cancellation))
                        {
                            if(_funcStopSignalDetector.Invoke(message))
                            {
                                disconnectReceived = true;
                            }
                            else
                            {
                                await _writer.WriteAsync(_converter.Invoke(message));
                            }
                        }
                    } while(!_cts.IsCancellationRequested && !disconnectReceived && !cancellation.IsCancellationRequested);
                }
                catch(OperationCanceledException){}
                finally
                {
                    _funcRemoveHandler.Invoke(_receiverId,this);    
                }
            }
        }

        /// <summary>
        /// A message Handler for adding messages to send to a queue 
        /// </summary>
        interface IMessageHandler : IMessageSender
        {
            /// <summary>
            /// Returns true if the Sending over that handler was canceled
            /// </summary>
            bool IsCanceled { get; }
            /// <summary>
            /// Cancels the sending over that handler
            /// </summary>
            void CancelAll();
            /// <summary>
            /// Add a Message to the queue
            /// </summary>
            /// <param name="message"></param>
            void AddMessage(TT message);
        }
    }

    /// <summary>
    /// A sender that provides messages for a receiver
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Gets a Task that is running as long there are messages to send
        /// </summary>
        /// <param name="cancellationToken">CancellationToken that allows to stop the Task that provides the messages</param>
        /// <returns>The Task where the Messages are send on</returns>
        Task ReceiveMessages(CancellationToken cancellationToken);
    }


    /// <summary>
    /// Notifications/Requests that can be send to a client
    /// </summary>
    public enum ClientCommandType
    {
        /// <summary>
        /// Invalid Request
        /// </summary>
        Unset,
        /// <summary>
        /// Client should send a Heartbeat
        /// </summary>
        SendHeartbeat,
        /// <summary>
        /// Client should synchronize the Server settings
        /// </summary>
        UpdateServerSettings,
        /// <summary>
        /// Client should synchronize the Client Settings
        /// </summary>
        UpdateClientSettings,
        /// <summary>
        /// Client should pickup a Login Intention
        /// </summary>
        GetLoginRequest,
        /// <summary>
        /// Client should update the current Session
        /// </summary>
        UpdateSession,
        /// <summary>
        /// Client should disconnect (server might go down)
        /// </summary>
        Disconnect,
    }
}
