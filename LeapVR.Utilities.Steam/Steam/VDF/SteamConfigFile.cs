#region Licence
/****************************************************************
 *  Filename: SteamConfigFile.cs
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
#endregion

namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class SteamConfigFile : VDFFile

    {
        public SteamConfigFile(string file_name) : base(file_name)
        {
        }

        public List<string> BaseInstallFolders
        {
            get
            {
                List<string> folders = new List<string>();
                NestedElement sele = SteamElement;
                if (sele != null)
                {
                    foreach (String name in sele.Children.Keys)
                    {
                        if (name.StartsWith("BaseInstallFolder"))
                        {
                            folders.Add(DeEscapeString(sele.Children[name].Value));
                        }
                    }
                }
                return folders;
            }
        }

        public Dictionary<String, SteamAccount> Users
        {
            get
            {
                Dictionary<String, SteamAccount> users = new Dictionary<String, SteamAccount>();
                NestedElement sele = SteamElement;
                if (sele != null && sele.Children.ContainsKey("Accounts"))
                {
                    NestedElement uele = sele.Children["Accounts"];
                    foreach (string name in uele.Children.Keys)
                    {
                        users.Add(name, new SteamAccount(uele.Children[name]));
                    }
                }
                return users;
            }
        }

        public NestedElement SteamElement
        {
            get
            {
                if (Elements.ContainsKey("InstallConfigStore"))
                {
                    NestedElement ele = Elements["InstallConfigStore"];
                    if (ele.Children.ContainsKey("Software"))
                    {
                        ele = ele.Children["Software"];
                        if (ele.Children.ContainsKey("Valve"))
                        {
                            ele = ele.Children["Valve"];
                            if (ele.Children.ContainsKey("Steam"))
                            {
                                ele = ele.Children["Steam"];
                                return ele;
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}