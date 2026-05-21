#region Licence
/****************************************************************
 *  Filename: IEMailAccountData.cs
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
using Pod.Data.Models.Mail;
using Pod.Enums;

namespace Pod.Data.Models.Interfaces 
{
    /// <summary>
    /// Interface for an EMail Account
    /// </summary>
    public interface IEMailAccountData 
    {
        string DisplayName { get; }
        string SenderName { get; }
        string EmailAddress { get;}
        string SmtpServer { get;  }
        int SmtpPort { get;  }
        bool UseSsl { get;  }
        bool IsEnabled { get; }
        string Username { get; }
        string Password { get; }
        SmtpAuthentication AuthMethod { get; }
    }
}