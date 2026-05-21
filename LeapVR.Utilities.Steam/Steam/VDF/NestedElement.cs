#region Licence
/****************************************************************
 *  Filename: NestedElement.cs
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
#region Using Directives
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
#endregion

namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class NestedElement
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, NestedElement> Children { get; protected set; }

        public String Name { get; protected set; }
        public String Value;

        public Boolean Ready { get; protected set; }


        private static Regex name_value_regex = new Regex("\"[^\"]*\"");
        private static Regex open_regex = new Regex("{");
        private static Regex close_regex = new Regex("}");

        public NestedElement(Queue<string> read)
        {
            Children = new Dictionary<string, NestedElement>();
            String line = read.Dequeue();
            MatchCollection matches = name_value_regex.Matches(line);

            if (string.IsNullOrWhiteSpace(line.TrimEnd('}')))
                return;

            if (matches.Count == 0)
            {
                throw new Exception("WHAT THE HELL");
            }

            Name = matches[0].Value.Trim('\"');
            if (matches.Count > 1)
            {
                Value = matches[1].Value.Trim('\"');
            }


            Ready = true;
            line = read.Peek();
            if (open_regex.IsMatch(line))
            {
                read.Dequeue();
                while (true)
                {
                    if (close_regex.IsMatch(read.Peek()))
                    {
                        Logger.Debug($"NestedElement >>>> inside while - SPECIAL return");
                        read.Dequeue();
                        return;
                    }
                    NestedElement ele = new NestedElement(read);
                    if (ele.Ready)
                    {
                        Children.Add(ele.Name, ele);
                    }
                    if (read.Count == 0 && line == "{")
                        break;

                    line = read.Peek();
                    if (close_regex.IsMatch(line))
                    {
                        read.Dequeue();
                        return;
                    }
                }
            }
        }
    }
}