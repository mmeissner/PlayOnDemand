#region Licence
/****************************************************************
 *  Filename: SendMailCommand.cs
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
using Pod.Enums;

namespace Pod.Services.Email 
{
    /// <summary>
    /// Command to Send an EMail
    /// </summary>
    public class SendMailCommand
    {
        /// <summary>
        /// Creates an new Email Send Command
        /// </summary>
        /// <param name="emailAccountId">The email Account Id to send the mail with</param>
        /// <param name="templateIdentifier">The Template Identifier for the template to use</param>
        /// <param name="toReceiverEMail">The Receiver EMail Address to send to</param>
        /// <param name="variableValues">The Variables to replace in the template</param>
        /// <param name="ccReceiverEMails">The CarbonCopy receiver email addresses to send to</param>
        /// <param name="bccReceiverEMails">The BlindCarbonCopy receiver email addresses to send to</param>
        public SendMailCommand(
                Guid emailAccountId,
                EMailTemplateIdentifier templateIdentifier,
                string toReceiverEMail,
                IReadOnlyDictionary<TemplateVariableKey, string> variableValues = null,
                ICollection<string> ccReceiverEMails = null,
                ICollection<string> bccReceiverEMails = null)
        {
            EmailAccountId = emailAccountId;
            ToReceiverEMail = toReceiverEMail;
            TemplateIdentifier = templateIdentifier;
            VariableValues = variableValues;
            VariableValues = variableValues ?? new Dictionary<TemplateVariableKey, string>();
            CcReceiverEMails = ccReceiverEMails ?? new HashSet<string>();
            BccReceiverEMails = bccReceiverEMails ?? new HashSet<string>();
        }
        /// <summary>
        /// The Id of the Email Account to use to send the Mail
        /// </summary>
        public Guid EmailAccountId { get; }

        /// <summary>
        /// The Identifier for the Template to use
        /// </summary>
        public EMailTemplateIdentifier TemplateIdentifier { get; }
        
        /// <summary>
        /// The receiver Email Address
        /// </summary>
        public string ToReceiverEMail { get; }

        /// <summary>
        /// The Carbon Copy receiver EMail Addresses
        /// </summary>
        public ICollection<string> CcReceiverEMails { get; }

        /// <summary>
        /// The Blind Carbon Copy receiver EMail Addresses
        /// </summary>
        public ICollection<string> BccReceiverEMails { get; }

        /// <summary>
        /// The Dictionary with the Template Variables and values to replace in the Template
        /// </summary>
        public IReadOnlyDictionary<TemplateVariableKey, string> VariableValues { get; }
    }
}