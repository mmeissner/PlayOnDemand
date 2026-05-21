#region Licence
/****************************************************************
 *  Filename: NestedElementFile.cs
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
using System.IO;
#endregion

namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class NestedElementFile
    {
        public Dictionary<string, NestedElement> Elements = new Dictionary<string, NestedElement>();

        public NestedElementFile(String file_name)
        {
            using(TextReader read = new StreamReader(file_name))
            {
                Queue<string> lines = new Queue<string>();
                String line = read.ReadLine();
                while (line != null)
                {
                    lines.Enqueue(line);
                    line = read.ReadLine();
                }

                while (lines.Count > 0)
                {
                    NestedElement ele = new NestedElement(lines);
                    if (ele.Ready)
                    {
                        Elements.Add(ele.Name, ele);
                    }
                }
            }
        }
    }
}