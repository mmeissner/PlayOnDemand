#region Licence
/****************************************************************
 *  Filename: ILetsEncryptHook.cs
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pod.LetsEncrypt
{
    public interface ILetsEncryptHook
    {
        string HookId { get; }
        /// <summary>
        /// An Hook can get called anywhere in the renewal process if an error occurs
        /// Otherwise it will be called after a Challenge is successfully requested
        /// and after a certificate is successfully renewed 
        /// </summary>
        /// <param name="args">Details about the Event</param>
        /// <returns></returns>
        Task LetsEncryptEvent(LetsEncryptEventArgs args);
    }

    /// <summary>
    /// Describes the Event
    /// </summary>
    public class LetsEncryptEventArgs
    {
        /// <summary>
        /// Indicates when this event was generated
        /// </summary>
        public LetsEncryptStage Stage { get; set; }
        /// <summary>
        /// The Domain Name the event was generated for
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// If an exception occured it would be here
        /// </summary>
        public Exception Exception { get; set; }

        public async Task SendArgs(ILogger logger, IEnumerable<ILetsEncryptHook> receivers)
        {
            foreach (ILetsEncryptHook hook in receivers)
            {
                try
                {
                    await hook.LetsEncryptEvent(this);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"An exception occured during executing a Hook with the Id= {hook.HookId}");
                }
            }
        }
    }

    /// <summary>
    /// Different states of a challenge order up to the renewal of a certificate
    /// </summary>
    public enum LetsEncryptStage
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Undefined,
        /// <summary>
        /// When a new Refresh Certificates cycle starts
        /// </summary>
        RefreshStarted,
        /// <summary>
        /// When an Account Key is about to be received or created
        /// </summary>
        GetAccountKey,
        /// <summary>
        /// When an Order for a Challenge is created
        /// </summary>
        CreateOrder,
        /// <summary>
        /// When a challenge was requested
        /// </summary>
        RequestChallenge,
        /// <summary>
        /// Then the challenge request gets acknowledged
        /// </summary>
        AcknowledgesChallenge,
        /// <summary>
        /// When a Challenge is performed
        /// </summary>
        ChallengeArrived,
        /// <summary>
        /// When a certificate is received and is about to converted into the right format
        /// </summary>
        BuildCertificate,
        /// <summary>
        /// When a certificate is about to be persisted
        /// </summary>
        PersistCertificate,
        /// <summary>
        /// When a certificate was successful renewed 
        /// </summary>
        CertificateRenewed
    }
}
