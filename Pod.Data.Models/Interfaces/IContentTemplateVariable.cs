#region Licence
/****************************************************************
 *  Filename: IContentTemplateVariable.cs
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
namespace Pod.Data.Models.Interfaces 
{
    /// <summary>
    /// Data for an variable found in an text
    /// </summary>
    public interface IContentTemplateVariable
    {
        /// <summary>
        /// The text representation of that variable
        /// </summary>
        string VariableKey { get; }

        /// <summary>
        /// The first char where the variable starts
        /// </summary>
        int StartChar { get;  }

        /// <summary>
        /// The length of the variable 
        /// </summary>
        int Length { get;  }
    }
}