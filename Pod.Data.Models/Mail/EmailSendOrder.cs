#region Licence
/****************************************************************
 *  Filename: EmailSendOrder.cs
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
using Pod.Data.Infrastructure;
using Pod.Enums;

namespace Pod.Data.Models.Mail
{
    /// <summary>
    /// An Order to send an EMail
    /// In combination with an Token this could be used to dynamically
    /// create Email Content to render in the Browser
    /// </summary>
    public class EmailSendOrder
    {
        private HashSet<EMailReceiver> _receivers;
        private HashSet<EMailVariableValue> _variables;

        private EmailSendOrder() { }
        private EmailSendOrder(
                EMailTemplateIdentifier templateIdentifier,
                HashSet<EMailReceiver> receivers,
                HashSet<EMailVariableValue> variables)
        {
            TemplateIdentifier = templateIdentifier;
            _receivers = receivers;
            _variables = variables;
            CreatedOnUtc = DateTime.UtcNow;
            SendState = EmailSendState.Unsend;
        }

        /// <summary>
        /// Id of the Send Mail Order
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// DateTime when the order was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// Last Time there was an attempt to deliver the mail
        /// </summary>
        public DateTime LastSendAttemptUtc { get; private set; }

        /// <summary>
        /// Identifier for the Template to use with this email
        /// </summary>
        public EMailTemplateIdentifier TemplateIdentifier { get; private set; }

        /// <summary>
        /// All Receivers for this email
        /// </summary>
        public IReadOnlyCollection<EMailReceiver> Receivers => _receivers;

        /// <summary>
        /// All Variables for replacing in the Template
        /// </summary>
        public IReadOnlyCollection<EMailVariableValue> Variables => _variables;

        /// <summary>
        /// The State of the Order
        /// </summary>
        public EmailSendState SendState { get; private set; }

        /// <summary>
        /// Amount of send attempts
        /// </summary>
        public uint SendAttempts { get; private set; }

        /// <summary>
        /// An Error Message in case of send failures
        /// </summary>
        public string ErrorMsg { get; private set; }

        /// <summary>
        /// Register the Send Attempt
        /// </summary>
        /// <param name="wasSuccess">The result of the attempt</param>
        /// <param name="maxTotalAttempts">The maximum amount or attempts allowed to try</param>
        /// <param name="errorMessage">An Error message in case of a unsuccessful send</param>
        public void SetSendAttemptResult(bool wasSuccess, int maxTotalAttempts, string errorMessage = null)
        {
            SendAttempts += 1;
            LastSendAttemptUtc = DateTime.UtcNow;
            if(wasSuccess)
            {
                SendState = EmailSendState.Send;
            }
            else
            {
                ErrorMsg = errorMessage;
                if(SendAttempts >= maxTotalAttempts)
                {
                    SendState = EmailSendState.Error;
                }
            }
        }

        public static IResult<EmailSendOrder> CreateOrder(
                EMailTemplateIdentifier templateIdentifier,
                ICollection<EMailReceiver> receivers,
                IReadOnlyDictionary<TemplateVariableKey, string> variableValues = null)
        {
            var result = new Result<EmailSendOrder>();
            if(!result.ArgNotNullOrEmpty(receivers, nameof(receivers), UserError.EMailNoReceiverSet))
            {
                return result;
            }
            var variables = new HashSet<EMailVariableValue>();
            if(variableValues != null)
            {
                foreach(var keyValuePair in variableValues)
                {
                    variables.Add(EMailVariableValue.Create(keyValuePair.Key, keyValuePair.Value));
                }
            }
            var hashSetReceivers = new HashSet<EMailReceiver>();
            bool hasToReceiver = false;
            foreach(EMailReceiver eMailReceiver in receivers)
            {
                var receiverResult = EMailReceiver.Create(
                        eMailReceiver.Type,
                        eMailReceiver.EmailAddress,
                        eMailReceiver.Name);
                result.Add(receiverResult);
                if(receiverResult.IsSuccess())
                {
                    if (eMailReceiver.Type == EmailReceiverType.To)
                    {
                        hasToReceiver = true;
                    }
                    hashSetReceivers.Add(receiverResult.ReturnValue);
                }
            }
            if(!result.ArgTrue(hasToReceiver, nameof(hasToReceiver), UserError.EMailNoReceiverSet)) return result;
            return result.Add(new EmailSendOrder(templateIdentifier, hashSetReceivers, variables));
        }
    }

    /// <summary>
    /// An Receiver for an Email
    /// </summary>
    public class EMailReceiver
    {
        private EMailReceiver() { }
        private EMailReceiver(EmailReceiverType type, string emailAddress)
        {
            Type = type;
            EmailAddress = emailAddress;
        }
        /// <summary>
        /// The Id of the Receiver
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The related <see cref="EmailSendOrder.Id"/>
        /// </summary>
        public Guid EmailSendOrderId { get; private set; }

        /// <summary>
        /// Navigation Property for <see cref="EmailSendOrder"/>
        /// </summary>
        public EmailSendOrder SendOrder { get; private set; }

        /// <summary>
        /// The Type of the Receiver
        /// </summary>
        public EmailReceiverType Type { get; private set; }

        /// <summary>
        /// The Email Address of the Receiver
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// The Name of the Receiver
        /// </summary>
        public string Name { get; private set; }

        public static IResult<EMailReceiver> Create(EmailReceiverType type, string emailAddress, string name = null)
        {
            var result = new Result<EMailReceiver>();
            result.ArgNotNullOrWhitespace(emailAddress, nameof(emailAddress), UserError.EMailAddressInvalid);
            if(result.HasError()) return result;
            var retval = new EMailReceiver(type,emailAddress);
            if(!string.IsNullOrWhiteSpace(name))
            {
                retval.Name = name;
            }
            return result.Add(retval);
        }
    }

    /// <summary>
    /// An Variable Value for an EMail Template
    /// </summary>
    public class EMailVariableValue
    {
        private EMailVariableValue() { }

        private EMailVariableValue(TemplateVariableKey key, string value)
        {
            Key = key;
            Value = value;
        }
        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The related <see cref="EmailSendOrder.Id"/>
        /// </summary>
        public Guid EmailSendOrderId { get; private set; }

        /// <summary>
        /// Navigation Property for <see cref="EmailSendOrder"/>
        /// </summary>
        public EmailSendOrder SendOrder { get; private set; }

        /// <summary>
        /// The Variable Key 
        /// </summary>
        public TemplateVariableKey Key { get; private set; }

        /// <summary>
        /// The Value for the Key
        /// </summary>
        public string Value { get; private set; }

        public static EMailVariableValue Create(TemplateVariableKey key, string value)
        {
            if(value == null) value = "";
            return new EMailVariableValue(key,value);
        }

    }
}