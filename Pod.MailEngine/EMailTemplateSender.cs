#region Licence
/****************************************************************
 *  Filename: EMailTemplateSender.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.MailEngine.Interfaces;

namespace Pod.MailEngine
{
    /// <summary>
    /// Can create and send Emails from on templates
    /// </summary>
    internal class EMailTemplateSender : IEmailTemplateSender
    {
        private readonly ILogger<IEmailTemplateSender> _logger;
        private readonly IEmailAccount _emailAccount;
        public EMailTemplateSender(ILogger<IEmailTemplateSender> logger, IEmailAccount emailAccount)
        {
            _logger = logger;
            _emailAccount = emailAccount;
        }

        /// <summary>
        /// Sends emails
        /// </summary>
        /// <param name="templates">Template collection of all the mails to send</param>
        /// <param name="cancellationToken">token to cancel sending</param>
        /// <returns>Result</returns>
        public async Task<IResult> SendEmailAsync(IEnumerable<EMailTemplate> templates, CancellationToken cancellationToken)
        {
            return await SendMail(templates, _emailAccount, cancellationToken);
        }

        /// <summary>
        /// Send a email
        /// </summary>
        /// <param name="template">Template to create mail from</param>
        /// <param name="cancellationToken">token to cancel sending</param>
        /// <returns>Result</returns>
        public async Task<IResult> SendEmailAsync(EMailTemplate template, CancellationToken cancellationToken)
        {
            return await SendMail(
                    new List<EMailTemplate> { template},
                    _emailAccount,
                    cancellationToken);
        }

        private async Task<IResult> SendMail(IEnumerable<EMailTemplate> templates, IEmailAccount mailingAccount, CancellationToken cancellationToken)
        {
            var result = new Result();
            try
            {
                var messages = new HashSet<MimeMessage>();
                foreach(var mailTemplate in templates)
                {
                    var buildMailResult = mailTemplate.BuildMail(mailingAccount.AccountData);
                    if(buildMailResult.HasError())
                    {
                        _logger.LogError($"Error during sending email: with template={mailTemplate.LogJson()}");
                        return result.Add(buildMailResult);
                    } 
                    messages.Add(buildMailResult.ReturnValue);
                }

                if(cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError($"Canceled during sending emails with templates ={messages.LogJson()}");
                    return result.Add("Cancellation during sending email!", UserError.EmailSendFailure);
                }

                if(!messages.Any()) return result;
                result.Add(await mailingAccount.ConnectAndSend(_logger, messages, cancellationToken));
            }
            catch(Exception exception)
            {
                _logger.LogError(exception, "Error during sending e-mail");
                result.Add(exception.Message, UserError.EmailSendFailure);
            }

            return result;
        }
    }
}