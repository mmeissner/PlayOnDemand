#region Licence
/****************************************************************
 *  Filename: IEmailTemplateSender.cs
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
using Pod.Data.Infrastructure;

namespace Pod.MailEngine.Interfaces 
{
    /// <summary>
    /// A sender that can send EMails based on Templates
    /// </summary>
    public interface IEmailTemplateSender
    {
        /// <summary>
        /// Send multiple Emails by Templates
        /// </summary>
        /// <param name="templates">The Email Templates to send</param>
        /// <param name="cancellationToken">A cancellation token to cancel sending</param>
        /// <returns>The send result</returns>
        Task<IResult> SendEmailAsync(IEnumerable<EMailTemplate> templates, CancellationToken cancellationToken);

        /// <summary>
        /// Send a email by a template
        /// </summary>
        /// <param name="template">The Template to send</param>
        /// <param name="cancellationToken">A cancellation token to cancel sending</param>
        /// <returns>The send result</returns>
        Task<IResult> SendEmailAsync(EMailTemplate template, CancellationToken cancellationToken);
    }
}