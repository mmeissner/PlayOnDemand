#region Licence
/****************************************************************
 *  Filename: IVariableParser.cs
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

namespace Pod.Data.Models.Interfaces 
{
    /// <summary>
    /// Parse a text and detects used variables in the text
    /// </summary>
    public interface IVariableParser
    {
        /// <summary>
        /// Parse a text for variables
        /// </summary>
        /// <param name="text">The Text to parse</param>
        /// <param name="variableControlChar">The control character that the variable starts and ends with</param>
        /// <returns>A collection of all detected variables</returns>
        ICollection<IContentTemplateVariable> Parse(string text, char variableControlChar);
    }
}