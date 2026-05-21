#region Licence
/****************************************************************
 *  Filename: UIPlatformAccountChanged.cs
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
using System.Threading.Tasks;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    /// <summary>
    /// Information about an Platform Account Change
    /// </summary>
    public interface IUIPlatformAccountChanged
    {
        /// <summary>
        /// Gets the application identifier affected.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        Guid? ApplicationId { get; }
        Guid PlatformId { get; }
        string AccountId { get; }
        AccountEventType Type { get; }
    }

    public class UIPlatformAccountChanged : IUIPlatformAccountChanged
    {
        public UIPlatformAccountChanged(Guid platformId, Guid? applicationId, string accountId, AccountEventType accountEventType)
        {
            ApplicationId = applicationId;
            PlatformId = platformId;
            AccountId = accountId;
            Type = accountEventType;
        }

        public Guid? ApplicationId { get; }
        public Guid PlatformId { get; }
        public string AccountId { get; }
        public AccountEventType Type { get; }
    }

    public enum AccountEventType
    {
        AddApps,
        AddAccount,
        RemoveApps,
        RemoveAccount,
    }
}
