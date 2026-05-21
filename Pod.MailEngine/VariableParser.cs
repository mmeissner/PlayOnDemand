#region Licence
/****************************************************************
 *  Filename: VariableParser.cs
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
using Pod.Data.Models.Interfaces;

namespace Pod.MailEngine 
{
    /// <summary>
    /// Can detect Variables in text that are escaped
    /// </summary>
    public class VariableParser : IVariableParser
    {
        /// <summary>
        /// Parses a text to detect variables in it
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="variableControlChar">The char that escapes the variables</param>
        /// <returns>Collection with found variables</returns>
        public ICollection<IContentTemplateVariable> Parse(string text, char variableControlChar)
        {
            var retval = new List<IContentTemplateVariable>();
            var stringBuilder = new StringBuilder(text);
            var variableKeyBuilder = new StringBuilder(50);
            int startChar = -1;

            for(int i = 0; i < stringBuilder.Length; i++)
            {
                var currentChar = stringBuilder[i];
                if(currentChar.Equals(variableControlChar))
                {
                    if(startChar == -1)
                    {
                        //First Character
                        startChar = i;
                    }
                    else
                    {
                        //End Char
                        retval.Add(new ContentTemplateVariable(variableKeyBuilder.ToString(),startChar,variableKeyBuilder.Length + 2));
                        //Reset
                        Reset();
                    }
                }
                else if(Char.IsLetterOrDigit(currentChar))
                {
                    //We still waiting for first control char
                    if(startChar == -1) continue;
                    else
                    {
                        //We are still evaluating a variable
                        variableKeyBuilder.Append(currentChar);
                    }
                }
                else
                {
                    if(startChar == -1) continue;
                    //Any non Letter or Digit stops the started variable detection
                    Reset();
                }
            }

            return retval;


            void Reset()
            {
                startChar = -1;
                variableKeyBuilder.Clear();
            }
        }

        /// <summary>
        /// Class to provide info about found variables
        /// </summary>
        class ContentTemplateVariable : IContentTemplateVariable
        {
            public ContentTemplateVariable(string variableKey, int startChar, int length)
            {
                VariableKey = variableKey;
                StartChar = startChar;
                Length = length;
            }
            /// <summary>
            /// The Identifier of the variable
            /// </summary>
            public string VariableKey { get; }
            
            /// <summary>
            /// Position in text where the variable starts
            /// </summary>
            /// 
            public int StartChar { get; }

            /// <summary>
            /// Position in text where the variable ends
            /// </summary>
            public int Length { get; }
        }
    }
}