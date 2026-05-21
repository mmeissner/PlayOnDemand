#region Licence
/****************************************************************
 *  Filename: SteamAppInfo.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Utilities.Steam.Steam.VDF.Binary;
using NLog;
using Pod.Data.Infrastructure;

namespace LeapVR.Utilities.Steam.Steam
{
    public class SteamAppInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public bool IsValid { get; }
        public bool IsInstalled { get; }
        public bool OpenVrSupport { get; } 
        public OsSupport SupportedOs { get; }
        public uint AppId { get; set; }
        public string DisplayName { get; }
        public string InstallDirName { get; }
        public string AppDirectory { get; }
        public List<SteamAppLaunchInfo> LaunchInfo { get; }
        public SteamAppInfo(VdfData gameData)
        {
            Logger.Trace("Analyze VdfData for game:{gamedData}",gameData.LogJson());
            LaunchInfo = new List<SteamAppLaunchInfo>();
            //Process Common
            bool gameSectionParseSuccess = false;
            if(gameData.TryGetValue("common", out var commonInfo))
            {
                if(commonInfo is VdfData vdfData)
                {
                    if(!vdfData.TryGetValue("name", out var name) || !vdfData.TryGetValue("gameid", out var gameId))
                    {
                        IsValid = false;
                        return;
                    }
                    var displayNameBytes = Encoding.Default.GetBytes(name.ToString());
                    DisplayName = Encoding.UTF8.GetString(displayNameBytes);
                    AppId = UInt32.Parse(gameId.ToString());
                    if(vdfData.TryGetValue("oslist", out var supportedOs))
                    {
                        SupportedOs = ParseString(supportedOs);
                    }

                    if(vdfData.TryGetValue("openvrsupport", out var supportOpenVr))
                    {
                        if(supportOpenVr.ToString() == "1")
                        {
                            OpenVrSupport = true;
                            Logger.Trace("OpenVR Support Detected");
                        }
                        else
                        {
                            Logger.Trace("No Support for OpenVR Detected");
                        }
                    }
                    else
                    {
                        Logger.Trace("Could not detect section for OpenVR support");
                    }

                    gameSectionParseSuccess = true;
                }
            }
            else
            {
                Logger.Debug("Game has no common section:{gamedData}",gameData.LogJson());
            }

            //Get AppDirectory
            if(SteamLib.AppInstallDirectories.ContainsKey(AppId)&& Directory.Exists(SteamLib.AppInstallDirectories[AppId]))
            {
                AppDirectory = SteamLib.AppInstallDirectories[AppId];
                IsInstalled = true;
                Logger.Debug($"Installed Game with Name={DisplayName}, AppId={AppId} detected!");
            }
            else
            {
                IsInstalled = false;
                Logger.Debug($"Game with Name={DisplayName}, AppId={AppId} seems to be not installed!");
            }
            // Config Section
            bool configSectionParseResult = false;
            if(gameData.TryGetValue("config", out var configInfoValue))
            {
                if(configInfoValue is VdfData vdfData)
                {
                    if(vdfData.TryGetValue("installdir", out var installDirectoryname) &&
                       vdfData.TryGetValue("launch", out var launchobj) &&
                       launchobj is VdfData launchInfo)
                    {
                        var bytes = Encoding.Default.GetBytes(installDirectoryname.ToString());
                        InstallDirName = Encoding.UTF8.GetString(bytes);

                        //If there is more then one Launch Option per OS we do not allow the Type of None
                        var allowTypeNone = launchInfo.Count(x=> x.Value is VdfData data && data.ContainsKey("executable") && data["executable"].ToString().ToLowerInvariant().EndsWith(".exe")) == 1;
                        if(allowTypeNone)
                        {
                            Logger.Debug("Allowing Type non as there is only one Launch Option for Windows");
                        }
                        foreach(object launchInfoObj in launchInfo.Values)
                        {
                            if(launchInfoObj is VdfData launchInfoData)
                            {
                                var appLaunchResult = SteamAppLaunchInfo.Parse(launchInfoData, AppDirectory, allowTypeNone);
                                if(appLaunchResult != null)
                                {
                                    configSectionParseResult = true;
                                    LaunchInfo.Add(appLaunchResult);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.Warn($"Config Section of Game with Name={DisplayName}, AppId={AppId} seems to be not of type{typeof(VdfData)}!");
                }
            }
            else
            {
                Logger.Warn($"Game with Name={DisplayName}, AppId={AppId} seems to have no config section!");
            }


            IsValid = configSectionParseResult && gameSectionParseSuccess;
            if(IsValid)
            {
                Logger.Info($"Valid Game with Name={DisplayName}, AppId={AppId}");
            }
            else
            {
                Logger.Debug($"Invalid Game with Name={DisplayName}, AppId={AppId}, Config Section Success={configSectionParseResult}, Game Section Success={gameSectionParseSuccess}");
            }
        }
        internal static OsSupport ParseString(object osObject)
        {
            OsSupport supportedOs = OsSupport.None;
            var osString = osObject.ToString();
            if(osString.Contains("windows")) supportedOs |= OsSupport.Windows;
            if(osString.Contains("macos")) supportedOs |= OsSupport.MacOs;
            if(osString.Contains("linux")) supportedOs |= OsSupport.Linux;
            return supportedOs;
        }
    }

    public class SteamAppLaunchInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        internal readonly bool IsValid;
        SteamAppLaunchInfo(VdfData launchInfo,string appDirectory, bool allowTypeNone)
        {
            //Config Section is Optional

            Logger.Debug($"Parsing for LaunchInfo for Game in AppDirectory={appDirectory}");
            if(launchInfo.TryGetValue("executable", out var executable))
            {
                launchInfo.TryGetValue("description", out var description);
                Description = description?.ToString();

                if(launchInfo.TryGetValue("config", out var config))
                {
                    if(config is VdfData configData)
                    {
                        if(configData.TryGetValue("oslist", out var osList))
                        {
                            SupportedOs = SteamAppInfo.ParseString(osList);
                            //We only interested in Windows supported LaunchInfo
                            if(!SupportedOs.HasFlag(OsSupport.Windows))return;
                        }
                        //Some Games have no oslist
                        else SupportedOs= OsSupport.Windows;
                    }
                    else
                    {
                        Logger.Warn($"Config is not of Type {typeof(VdfData)}!");
                    }
                }
                else
                {
                    //If there is no Config or no Supported OS detected we default to windows
                    SupportedOs= OsSupport.Windows;
                }
               

                Executable = executable.ToString();
                if(appDirectory != null) ExecutableFilePath = Path.Combine(appDirectory, Executable);
                else ExecutableFilePath = Executable;
                if(!File.Exists(ExecutableFilePath))
                {
                    // Benign in nearly all cases: a Steam app's appinfo.vdf
                    // typically carries multiple launch entries (Screen / VR /
                    // Oculus / Linux / playtest) and only one matches the local
                    // install. Logging each non-installed alternate at Warn
                    // produced ~10x noise per game; the parent SteamAppInfo
                    // emits a higher-level "Invalid Game" line if ALL options
                    // fail to validate.
                    Logger.Debug($"Skipping launch option with missing executable at Path={ExecutableFilePath}");
                    return;
                }

                Logger.Debug($"Detected Executable at Path ={ExecutableFilePath}");


                if(String.IsNullOrWhiteSpace(Description))
                {
                    Logger.Debug("Description of Launch Option not available, using default");
                    Description = "Default";
                }

                if(launchInfo.TryGetValue("arguments", out var arguments))
                {
                    Arguments = arguments.ToString();
                }

                IsValid = true;
                Logger.Debug("Trying to identify Type of Launch Option");
                if(launchInfo.TryGetValue("type", out var type))
                {
                    var sType = type.ToString();
                    switch(sType)
                    {
                        //Seems to be for SteamVR/OpenVR
                        case "vr":
                            Type = LaunchType.OpenVr;
                            break;

                        //Can be any other VR
                        case "othervr":
                            if(Description != null && Description.ToLowerInvariant().Contains("oculus") ||
                               Arguments != null && Arguments.ToLowerInvariant().Contains("oculus"))
                            {
                                Type = LaunchType.Oculus;
                            }
                            else
                            {
                                Type = LaunchType.OtherVr;
                            }
                            break;

                        //Seems to be used for generic valid startup option
                        case "default":
                            Type = LaunchType.Screen;
                            break;

                        //Startup Options for internal use ??
                        case "none":
                            if(allowTypeNone)
                            {
                                Type = LaunchType.Screen;
                            }
                            else
                            {
                                IsValid = false;
                            }
                            break;
                        default:
                            IsValid = false;
                            break;
                    }
                    Logger.Debug($"Launch Type was set to={Type}");
                }
                else
                {
                    Logger.Debug("Could not get Type Information, using Screen Game as default");
                    Type = LaunchType.Screen;
                }
            }
            else
            {
                Logger.Warn("Could not find executable and config section!");
            }
        }
        public string Executable { get; }
        public string ExecutableFilePath { get; }
        public string Arguments { get; }
        public string Description { get; }
        public OsSupport SupportedOs { get; }
        public LaunchType Type { get; }
        public static SteamAppLaunchInfo Parse(VdfData launchInfoData, string appDirectory, bool allowTypeNone)
        {
            var retval = new SteamAppLaunchInfo(launchInfoData,appDirectory,allowTypeNone);
            if(!retval.IsValid) return null;
            return retval;
        }
    }

    [Flags]
    public enum OsSupport
    {
        None = 1,
        Windows = 2,
        MacOs = 4,
        Linux = 8
    }
    public enum LaunchType
    {
        Screen,
        OpenVr,
        Oculus,
        OtherVr
    }
}