#region Licence
/****************************************************************
 *  Filename: Program.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Utilities.VersionInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                try
                {
                    string path = Path.GetFullPath(arg);
                    var assembly = Assembly.LoadFrom(path);
                    Console.Out.WriteLine(assembly.GetName().Version.ToString());
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"{arg}: {exception.Message}");
                }
            }
        }
    }
}
