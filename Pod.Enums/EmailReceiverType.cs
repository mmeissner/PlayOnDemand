#region Licence
/****************************************************************
 *  Filename: EmailReceiverType.cs
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
namespace Pod.Enums 
{
    /// <summary>
    /// Enumeration of different email receiver types
    /// </summary>
    public enum EmailReceiverType
    {
        //The main receiver
        To,
        //Other receivers can not see who has received a blind copy
        BlindCarbonCopy,
        //Everyone can see who has received an Carbon Copy
        CarbonCopy
    }


    public enum EmailSendState
    {
        /// <summary>
        /// Email was not yet send out
        /// </summary>
        Unsend,
        /// <summary>
        /// EMail was send
        /// </summary>
        Send,
        /// <summary>
        /// EMail cant be send and has Error
        /// </summary>
        Error,
    }
}