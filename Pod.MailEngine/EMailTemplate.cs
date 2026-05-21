#region Licence
/****************************************************************
 *  Filename: EMailTemplate.cs
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
using MimeKit;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Mail;
using Pod.Enums;
using Pod.MailEngine.Interfaces;

namespace Pod.MailEngine
{
    /// <summary>
    /// Allows to send E-Mails based on a <see cref="EmailContentTemplate"/>
    /// </summary>
    public class EMailTemplate
    {
        private readonly EmailContentTemplate _emailContentTemplate;
        private readonly Dictionary<TemplateVariableKey, string> _variablesValuesDic = new Dictionary<TemplateVariableKey, string>();
        private readonly HashSet<MailboxAddress> _bccReceivers = new HashSet<MailboxAddress>();
        private readonly HashSet<MailboxAddress> _ccReceivers = new HashSet<MailboxAddress>();
        private MailboxAddress _toReceiver;

        public EMailTemplate(EmailContentTemplate emailContentTemplate) { _emailContentTemplate = emailContentTemplate; }
        public EMailTemplate(EmailContentTemplate emailContentTemplate, Dictionary<TemplateVariableKey, string> defaultVariableDictionary)
        {
            _emailContentTemplate = emailContentTemplate;
            _variablesValuesDic = new Dictionary<TemplateVariableKey, string>(defaultVariableDictionary);
        }
        
        /// <summary>
        /// The identifier this template is assigned to
        /// This is a fixed value that is used in multiple places where emails of specific purpose needs to be send
        /// The identifier is related to such a purpose
        /// </summary>
        public EMailTemplateIdentifier Identifier => _emailContentTemplate.Identifier;

        /// <summary>
        /// Add an Receiver for the email
        /// </summary>
        /// <param name="name">The name of the receiver</param>
        /// <param name="emailAddress">the email address to send to</param>
        /// <param name="emailReceiverType">The type of the receiver</param>
        public void AddReceiver(string name, string emailAddress, EmailReceiverType emailReceiverType = EmailReceiverType.To)
        {
            if(string.IsNullOrWhiteSpace(name)) AddReceiver(emailAddress, emailReceiverType);
            else AddReceiver(emailReceiverType, new MailboxAddress(Encoding.UTF8, name, emailAddress));
        }

        /// <summary>
        /// Add an Receiver for the email
        /// </summary>
        /// <param name="emailAddress">the email address to send to</param>
        /// <param name="emailReceiverType">The type of the receiver</param>
        public void AddReceiver(string emailAddress, EmailReceiverType emailReceiverType = EmailReceiverType.To)
        {
            AddReceiver(emailReceiverType,new MailboxAddress(emailAddress));
        }
        
        /// <summary>
        /// Removes all assigned receivers
        /// </summary>
        public void ClearReceivers()
        {
            _toReceiver = null;
            _bccReceivers.Clear();
            _ccReceivers.Clear();
        }
        
        /// <summary>
        /// Removes specific kind of receivers
        /// </summary>
        /// <param name="type">The type of receivers to remove</param>
        public void ClearReceivers(EmailReceiverType type)
        {
            switch (type)
            {
                case EmailReceiverType.To:
                    _toReceiver = null;
                    break;
                case EmailReceiverType.BlindCarbonCopy:
                    _bccReceivers.Clear();
                    break;
                case EmailReceiverType.CarbonCopy:
                    _ccReceivers.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        /// <summary>
        /// Adds the values for template variables to be later replaced
        /// </summary>
        /// <param name="key">The variable identifier</param>
        /// <param name="value">The value</param>
        /// <returns>result</returns>
        public IResult SetOrReplaceVariable(TemplateVariableKey key, string value)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(value, nameof(value), UserError.TemplateInvalidVariable);
            if(result.HasError()) return result;
            _variablesValuesDic[key] = value;
            return result;
        }
        /// <summary>
        /// Adds multiple values for template variables to be later replaced
        /// </summary>
        /// <param name="variables">Dictionary with all variables</param>
        /// <returns>result</returns>
        public IResult SetOrReplaceVariable(IReadOnlyDictionary<TemplateVariableKey,string> variables)
        {
            var result = new Result();
            foreach(KeyValuePair<TemplateVariableKey, string> variable in variables)
            {
                result.Add(SetOrReplaceVariable(variable.Key, variable.Value));
            }
            return result;
        }

        /// <summary>
        /// Allows to remove a specific variable
        /// </summary>
        /// <param name="key"></param>
        public void RemoveVariable(TemplateVariableKey key)
        {
            _variablesValuesDic.Remove(key);
        }
        
        /// <summary>
        /// Setup an email with its content and receivers
        /// </summary>
        /// <param name="accountData">The senders email data</param>
        /// <returns>Send-able message</returns>
        internal IResult<MimeMessage> BuildMail(IEMailAccountData accountData)
        {
            var result = new Result<MimeMessage>();

            //Must have a to receiver
            if(!result.ArgNotNull(_toReceiver, nameof(_toReceiver)))
            {
                return result;
            }

            //Try to get sender
            if(!result.ArgNotNullOrWhitespace(accountData.EmailAddress, nameof(accountData.EmailAddress)))
            {
                return result;
            }
            var message = new MimeMessage();

            //Set a sender Name if one was provided
            if(string.IsNullOrWhiteSpace(accountData.SenderName))
            {
                message.From.Add(new MailboxAddress(accountData.EmailAddress));
            }
            else
            {
                message.From.Add(new MailboxAddress(Encoding.UTF8, accountData.SenderName, accountData.EmailAddress));
            }

            //Add Contents
            result.Add(SetMailContent(message, _emailContentTemplate,_variablesValuesDic));
            if(result.HasError()) return result;

            //Add receivers
            message.To.Add(_toReceiver);
            foreach(MailboxAddress bccReceiver in _bccReceivers)
            {
                message.Bcc.Add(bccReceiver);
            }

            foreach(MailboxAddress ccReceiver in _ccReceivers)
            {
                message.Cc.Add(ccReceiver);
            }
            return result.Add(message);
        }

        private void AddReceiver(EmailReceiverType type, MailboxAddress mailboxAddress)
        {
            switch (type)
            {
                case EmailReceiverType.To:
                    _toReceiver = mailboxAddress;
                    break;
                case EmailReceiverType.BlindCarbonCopy:
                    _bccReceivers.Add(mailboxAddress);
                    break;
                case EmailReceiverType.CarbonCopy:
                    _ccReceivers.Add(mailboxAddress);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Set the mail content
        /// </summary>
        /// <param name="message">The mime message where to set the content</param>
        /// <param name="emailContentTemplate">The template to use</param>
        /// <param name="variablesValuesDic">The variable values to replace in the template</param>
        /// <returns>result</returns>
        private static IResult SetMailContent(MimeMessage message, EmailContentTemplate emailContentTemplate, Dictionary<TemplateVariableKey, string> variablesValuesDic)
        {
            var result = new Result();
            var builder = new BodyBuilder();
            var subjectResult = ReplaceVariables(emailContentTemplate.SubjectText, emailContentTemplate.Variables.Where(x => x.Type == EmailVariableType.Subject), emailContentTemplate.VariableControlChar, variablesValuesDic);
            
            //Add Subject to Message
            result.Add(subjectResult);
            message.Subject = subjectResult.ReturnValue;
            if (emailContentTemplate.ContentText != null)
            {
                var contentTextResult = ReplaceVariables(emailContentTemplate.ContentText, emailContentTemplate.Variables.Where(x => x.Type == EmailVariableType.Content),emailContentTemplate.VariableControlChar, variablesValuesDic);
                result.Add(contentTextResult);
                builder.TextBody = contentTextResult.ReturnValue;
            }

            if(emailContentTemplate.ContentHtml != null)
            {
                var contentHtmlResult = ReplaceVariables(emailContentTemplate.ContentHtml, emailContentTemplate.Variables.Where(x => x.Type == EmailVariableType.ContentHtml), emailContentTemplate.VariableControlChar, variablesValuesDic);
                result.Add(contentHtmlResult);
                builder.HtmlBody = contentHtmlResult.ReturnValue;
            }

            //Check for any error
            if(result.HasError()) return result;

            //Add Body to Message
            message.Body = builder.ToMessageBody();
            return result;
        }

        /// <summary>
        /// Replaces variables in text
        /// </summary>
        /// <param name="text">Input Text with variables</param>
        /// <param name="variables">The variables to find in the text</param>
        /// <param name="variableControlChar">The escape char for the variables</param>
        /// <param name="variablesValuesDic">The dictionary holding variables and identifiers</param>
        /// <returns></returns>
        private static IResult<string> ReplaceVariables(string text, IEnumerable<EmailVariable> variables, char variableControlChar, Dictionary<TemplateVariableKey, string> variablesValuesDic)
        {
            //No replacement needed if there are no variables to replace
            var result = new Result<string>();

            //Use a String builder to improve replacement of many variables
            StringBuilder templateBuilder = new StringBuilder(text);
            foreach (EmailVariable variable in variables)
            {
                if (!variablesValuesDic.ContainsKey(variable.VariableKey))
                {
                    result.Add($"The required variable with key={variable.VariableKey} is not set by the system and cant be inserted into the template");
                    continue;
                }
                templateBuilder.Replace($"{variableControlChar}{variable.VariableKeyString}{variableControlChar}", variablesValuesDic[variable.VariableKey]);
            }
            if (result.HasError()) return result;
            return result.Add(templateBuilder.ToString());
        }
    }
}
