#region Licence
/****************************************************************
 *  Filename: EMailAccountData.cs
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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Enums;

namespace Pod.Data.Models.Mail
{
    /// <summary>
    /// Data for an Email Account
    /// </summary>
    public class EMailAccountData : IEMailAccountData
    {
        private HashSet<EMailAccountDataEMailContentTemplate> _emailContentTemplates;
        private EMailAccountData() { }
        private EMailAccountData(string displayName)
        {
            _emailContentTemplates = new HashSet<EMailAccountDataEMailContentTemplate>();
            DisplayName = displayName;
        }
        public Guid Id { get; private set; }

        /// <summary>
        /// A Display name for the email account
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The Name for the Sender e.g. Support, Info..
        /// </summary>
        public string SenderName { get; private set; }

        /// <summary>
        /// The E-Mail Address from the sender
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// The Address of the SMTP Server
        /// </summary>
        public string SmtpServer { get; private set; }

        /// <summary>
        /// The Port of the SMTP Server
        /// </summary>
        public int SmtpPort { get; private set; }

        /// <summary>
        /// If SSL should be used to connect
        /// </summary>
        public bool UseSsl { get; private set; }

        /// <summary>
        /// If the Account can be used to send Mails
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// The Authentication Method to use for the Smtp Server
        /// </summary>
        public SmtpAuthentication AuthMethod { get; set; }

        /// <summary>
        /// The Username for the Account or the OAuth2 Client Id
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The Password for the Account or the OAuth2 Secret
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Collection of linked EMail Templates that can be used with this account
        /// </summary>
        public IReadOnlyCollection<EMailAccountDataEMailContentTemplate> EmailContentTemplates =>
                _emailContentTemplates;

        /// <summary>
        /// Adds an Email Account Template
        /// </summary>
        /// <param name="emailContentTemplate">The template to add</param>
        /// <returns>result</returns>
        public IResult AddEMailContentTemplate(EmailContentTemplate emailContentTemplate)
        {
            var result = new Result();
            result.RefNotNull(_emailContentTemplates, nameof(_emailContentTemplates));
            //Check if not already there and prevent double entries
            if(result.IsSuccess())
            {
                if(emailContentTemplate.Id != Guid.Empty)
                {
                    if(_emailContentTemplates.Any(x => x.EMailContentTemplateId == emailContentTemplate.Id))
                    {
                        return result;
                    }
                }
                else
                {
                    if(_emailContentTemplates.Any(x => x.EmailContentTemplate.Equals(emailContentTemplate)))
                    {
                        return result;
                    }
                }
                _emailContentTemplates.Add(new EMailAccountDataEMailContentTemplate(this, emailContentTemplate));
            }
            return result;
        }

        /// <summary>
        /// Removes a Content Template for the EMailAccount so that it should not be used anymore with this account
        /// </summary>
        /// <param name="templateId">The Id of the <see cref="EmailContentTemplate"/> to remove</param>
        /// <returns>A result, if the result is success and does not return a content template, then there is no <see cref="EMailAccountDataEMailContentTemplate"/> to be deleted from the db</returns>
        public IResult<EMailAccountDataEMailContentTemplate> RemoveEMailContentTemplateBy(Guid templateId)
        {
            var result = new Result<EMailAccountDataEMailContentTemplate>();
            //Is Template already saved
            result.ValueIdValid(templateId,nameof(templateId));
            if(result.HasError()) return result;
            var template = _emailContentTemplates.FirstOrDefault(x => x.EMailContentTemplateId == templateId);
            if (template != null)
            {
                _emailContentTemplates.Remove(template);
                result.Add(template);
            }
            return result;
        }

        /// <summary>
        /// Creates an new Email Account that is non functional at the time of creation
        /// </summary>
        /// <param name="displayName">The display name for the account</param>
        /// <returns>result</returns>
        public static IResult<EMailAccountData> Create(string displayName)
        {
            var result = new Result<EMailAccountData>();
            result.ArgNotNullOrWhitespace(displayName, nameof(displayName), UserError.EMailAccountNameNotSet);
            if(result.HasError()) return result;
            return result.Add(new EMailAccountData(displayName));
        }

        /// <summary>
        /// Creates an Email account that can be functional at the time of the creation
        /// </summary>
        /// <param name="displayName">The display name for the account</param>
        /// <param name="emailAddress">The Email Address to use for this account</param>
        /// <param name="smtpServer"></param>
        /// <param name="smtpPort"></param>
        /// <param name="useSsl"></param>
        /// <param name="authMethod"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="senderName"></param>
        /// <returns></returns>
        public static IResult<EMailAccountData> CreateSmtp(
                string displayName, string emailAddress, string smtpServer, int smtpPort, bool useSsl,
                SmtpAuthentication authMethod, string username,
                string password, string senderName = null)
        {
            var result = Create(displayName);
            if(result.HasError()) return result;
            var returnVal = new Result<EMailAccountData>();
            returnVal.Add(result.ReturnValue.SetSender(emailAddress, senderName));
            returnVal.Add(result.ReturnValue.SetSmtpServer(smtpServer, smtpPort, useSsl));
            returnVal.Add(result.ReturnValue.SetSmtpAuth(username, password, authMethod));
            if(returnVal.IsSuccess()) returnVal.Add(result.ReturnValue);
            return returnVal;
        }

        /// <summary>
        /// Toggles the Enable state
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        public IResult SetEnabled(bool isEnabled)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(EmailAddress, nameof(EmailAddress), UserError.EMailAccountDataInvalid);
            result.ArgNotNullOrWhitespace(SmtpServer, nameof(SmtpServer), UserError.EMailAccountDataInvalid);
            result.ArgNotNullOrWhitespace(Username, nameof(Username), UserError.EMailAccountDataInvalid);
            result.ArgNotNullOrWhitespace(Password, nameof(Password), UserError.EMailAccountDataInvalid);
            result.ArgNotLowerOrEqualThen(
                    SmtpPort,
                    nameof(SmtpPort),
                    0,
                    "Valid Port",
                    UserError.EMailAccountDataInvalid);
            result.ArgNotHigherThen(
                    SmtpPort,
                    nameof(SmtpPort),
                    65535,
                    "Valid Port",
                    UserError.EMailAccountDataInvalid);
            if (result.IsSuccess())
            {
                IsEnabled = isEnabled;
            }

            return result;
        }

        /// <summary>
        /// Sets or Updates an Sender for Emails from this account
        /// </summary>
        /// <param name="emailAddress">The Email Address to use</param>
        /// <param name="senderName">The Senders name to set</param>
        /// <returns></returns>
        public IResult SetSender(string emailAddress, string senderName = null)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(emailAddress, nameof(emailAddress), UserError.EMailAccountDataInvalid);
            if(result.HasError()) return result;
            EmailAddress = emailAddress;
            SenderName = senderName;
            return result;
        }

        /// <summary>
        /// Sets or Updates an Smtp Server for this account
        /// </summary>
        /// <param name="smtpServer">The Url of the server</param>
        /// <param name="smtpPort">The Port to connect to the server</param>
        /// <param name="useSSl">If to use SSL</param>
        /// <returns></returns>
        public IResult SetSmtpServer(string smtpServer, int smtpPort, bool useSSl)
        {
            var result = new Result();
            result.ArgNotLowerOrEqualThen(
                    smtpPort,
                    nameof(smtpPort),
                    0,
                    "Valid PortValues",
                    UserError.EMailAccountDataInvalid);
            result.ArgNotHigherThen(
                    smtpPort,
                    nameof(smtpPort),
                    65535,
                    "Valid PortValues",
                    UserError.EMailAccountDataInvalid);
            if(result.HasError()) return result;
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            UseSsl = useSSl;
            return result;
        }
        
        /// <summary>
        /// Sets Authentication information for the Mail Server
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="authentication">Type of Authentication</param>
        /// <returns></returns>
        public IResult SetSmtpAuth(string username, string password, SmtpAuthentication authentication)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.EMailAccountDataInvalid);
            result.ArgNotNullOrWhitespace(password, nameof(password), UserError.EMailAccountDataInvalid);
            if(result.HasError()) return result;
            Username = username;
            Password = password;
            AuthMethod = authentication;
            return result;
        }
    }

    /// <summary>
    /// Linking instance for ManyToMany relation
    /// between <see cref="EMailAccountData"/> and <see cref="EmailContentTemplate"/>
    /// </summary>
    public class EMailAccountDataEMailContentTemplate
    {
        private EMailAccountDataEMailContentTemplate() { }
        public EMailAccountDataEMailContentTemplate(
                EMailAccountData eMailAccountData, EmailContentTemplate contentTemplate)
        {
            EMailAccountData = eMailAccountData;
            EmailContentTemplate = contentTemplate;
        }
        
        public Guid EMailAccountDataId { get; private set; }
        public EMailAccountData EMailAccountData { get; private set; }
        public Guid EMailContentTemplateId { get; private set; }
        public EmailContentTemplate EmailContentTemplate { get; private set; }
    }
}