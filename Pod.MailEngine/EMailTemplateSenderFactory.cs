#region Licence
/****************************************************************
 *  Filename: EMailTemplateSenderFactory.cs
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
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Enums;
using Pod.MailEngine.Interfaces;

namespace Pod.MailEngine 
{
    /// <summary>
    /// Creates Email Sender for Templates from Email AccountData
    /// </summary>
    public class EMailTemplateSenderFactory
    {
        private readonly ILogger<IEmailTemplateSender> _logger;

        /// <summary>
        /// Creates the Email sender
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="lifetime">Will trigger a cancellation for sending mails if application stops</param>
        public EMailTemplateSenderFactory(ILogger<IEmailTemplateSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates an Template sender
        /// </summary>
        /// <param name="accountData">The account data to use with</param>
        /// <returns>The Result of the fabrication</returns>
        public IResult<IEmailTemplateSender> Create(IEMailAccountData accountData)
        {
            var result = new Result<IEmailTemplateSender>();
            IResult<IEmailAccount> mailAccountResult = null;

            if(result.Add(IsAccountDataComplete(accountData)).IsSuccess())
            {
                if (accountData.EmailAddress.ToLowerInvariant().Contains("@gmail.com") ||
                    accountData.EmailAddress.ToLowerInvariant().Contains("@googlemail.com"))
                {
                    mailAccountResult = GMailAccount.Create(accountData);
                }
                else
                {
                    if (accountData.AuthMethod == SmtpAuthentication.OAuth2)
                    {
                        return new Result<IEmailTemplateSender>().
                                Add("Authentication Type of OAuth2 is currently only supported for google accounts", UserError.EMailAccountDataInvalid);
                    }
                    mailAccountResult = SmtpAccount.Create(accountData);
                }
            }
            if(mailAccountResult != null)
            {
                if(mailAccountResult.IsSuccess())
                {
                    result.Add(new EMailTemplateSender(_logger, mailAccountResult.ReturnValue));
                }
                else result.Add(mailAccountResult);
            }
            return result;
        }

        private IResult IsAccountDataComplete(IEMailAccountData accountData)
        {
            var retval = new Result();
            retval.ArgNotNullOrWhitespace(
                    accountData.SmtpServer,
                    nameof(accountData.SmtpServer),
                    UserError.EMailAccountDataInvalid);
            retval.ArgOutOfRange(accountData.SmtpPort, nameof(accountData.SmtpPort), 1, 65535, UserError.EMailAccountDataInvalid);
            retval.ArgNotNullOrWhitespace(
                    accountData.EmailAddress,
                    nameof(accountData.EmailAddress),
                    UserError.EMailAccountDataInvalid);
            retval.ArgNotNullOrWhitespace(
                    accountData.Username,
                    nameof(accountData.Username),
                    UserError.EMailAccountDataInvalid);
            retval.ArgNotNullOrWhitespace(
                    accountData.Password,
                    nameof(accountData.Password),
                    UserError.EMailAccountDataInvalid);
            return retval;
        }
    }
}