#region Licence
/****************************************************************
 *  Filename: IEmailAccount.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;

namespace Pod.MailEngine.Interfaces 
{
    /// <summary>
    /// Interface for a EMail Account were mails can be send with
    /// </summary>
    internal interface IEmailAccount
    {
        /// <summary>
        /// The EMail Account information to connect and authenticate with
        /// </summary>
        IEMailAccountData AccountData { get; }
        
        /// <summary>
        /// Method to send messages through this email account
        /// </summary>
        /// <param name="logger">The Logger to use to log</param>
        /// <param name="messages">The messages to send</param>
        /// <param name="cancellationToken">Token to cancel the send request</param>
        /// <returns></returns>
        Task<IResult> ConnectAndSend(ILogger<IEmailTemplateSender> logger, HashSet<MimeMessage> messages, CancellationToken cancellationToken);
    }
}