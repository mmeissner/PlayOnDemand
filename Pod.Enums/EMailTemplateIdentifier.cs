#region Licence
/****************************************************************
 *  Filename: EMailTemplateIdentifier.cs
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
    /// Collection of Identifiers by that Email Templates are located
    /// </summary>
    public enum EMailTemplateIdentifier
    {
        /// <summary>
        /// When an new Account is registered
        /// </summary>
        RegisterAccount,
        /// <summary>
        /// When an Email Confirmation is requested
        /// </summary>
        ResendEMailVerification,
        ResetPassword,
    }
}