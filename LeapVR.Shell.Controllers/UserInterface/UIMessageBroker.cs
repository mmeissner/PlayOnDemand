#region Licence
/****************************************************************
 *  Filename: UIMessageBroker.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.UserInterface;
using NLog;
using LogManager = Caliburn.Micro.LogManager;

namespace LeapVR.Shell.Controllers.UserInterface
{
    public class UIMessageBroker : IUIMessageBroker
    {
        #region Fields & Properties
        private readonly IEventAggregator _eventAggregator;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        public UIMessageBroker(IEventAggregator eventAggregator) { _eventAggregator = eventAggregator; }
        #endregion

        #region Methods
        #endregion

        public void Publish(object message)
        {
            try
            {
                var messageGuid = Guid.NewGuid();
                Logger.Debug($"Started Publishing Message of Type={message.GetType()}, MessageId={messageGuid}");
                _eventAggregator.PublishOnUIThread(message);
                Logger.Debug($"Finished Publishing Message of Type={message.GetType()}, MessageId={messageGuid}");
            }

            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Error(e, "Task Cancel Exception during Publishing");
                }
                else
                {
                    Logger.Error(e, "Unkown Exception during Publishing");
                    throw;
                }
            }
        }
        public async Task PublishAsync(object message)
        {
            try
            {
                var messageGuid = Guid.NewGuid();
                Logger.Debug($"Started Publishing Message of Type={message.GetType()}, MessageId={messageGuid}");
                await _eventAggregator.PublishOnUIThreadAsync(message);
                Logger.Debug($"Finished Publishing Message of Type={message.GetType()}, MessageId={messageGuid}");
            }

            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    Logger.Error(e, "Task Cancel Exception during Publishing");
                }
                else
                {
                    Logger.Error(e, "Unkown Exception during Publishing");
                    throw;
                }
            }
        }
        public void Subscribe(object subscriber)
        {
            Logger.Debug($"Start Subscribe by Type={subscriber.GetType()}");
            _eventAggregator.Subscribe(subscriber);
            Logger.Debug($"Finished Subscribe by Type={subscriber.GetType()}");
        }

        public void Unsubscribe(object subscriber)
        {
            Logger.Debug($"Start Unsubscribe by Type={subscriber.GetType()}");
            _eventAggregator.Unsubscribe(subscriber);
            Logger.Debug($"Finished Unsubscribe by Type={subscriber.GetType()}");
        }
    }
}