#region Licence
/****************************************************************
 *  Filename: SteamLib.cs
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Win;
using LeapVR.Utilities.Steam.Steam.VDF;
using LeapVR.Utilities.Steam.Steam.VDF.Binary;
using LeapVR.Utilities.Windows;
using NLog;
using Pod.Data.Infrastructure;
using Steam.Models.SteamStore;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Models;

namespace LeapVR.Utilities.Steam.Steam
{
    public class SteamLib
    {
        #region Private Fields
        private const string SteamLibraryFile = @"libraryfolders.vdf";
        private const string SteamAppInfoFile = @"appcache\appinfo.vdf";
        private const uint SteamVRAppId = 250820;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan WebRequestTimeout = TimeSpan.FromSeconds(10);
        // Header images are served by akamai/fastly CDNs that are occasionally
        // sluggish on cold-hit; give them their own, longer budget.
        private static readonly TimeSpan ImageDownloadTimeout = TimeSpan.FromSeconds(20);
        private const int ImageDownloadRetryAttempts = 1;
        private static readonly Dictionary<uint, string> _appInstallDirectories = new Dictionary<uint, string>();

        // Steam's public appdetails endpoint is aggressively rate-limited (~200
        // requests / 5min / IP). The kiosk's PlatformAppViewModel fires
        // GetOrUpdateDisplayDataAsync per app in parallel, which on a 63-game
        // library produces ~63 concurrent requests in <10s - well past Steam's
        // soft cap. The result: most requests time out, the kiosk shows nothing.
        //
        // Cap the kiosk's outbound rate with a small global semaphore + a
        // minimum spacing between dispatches. Tunables picked from empirical
        // probing: 2 concurrent requests + 150ms gap keeps the API happy for
        // libraries up to a few hundred games.
        private static readonly SemaphoreSlim _appDetailsConcurrency = new SemaphoreSlim(2, 2);
        private static readonly TimeSpan AppDetailsMinGap = TimeSpan.FromMilliseconds(150);
        private static DateTime _appDetailsLastDispatchUtc = DateTime.MinValue;
        private static readonly object _appDetailsGate = new object();
        private const int AppDetailsRetryAttempts = 2;

        private static string _steamPath;
        private static string _steamExeFilePathName;
        private static string _steamExe;
        private static string _steamProcessName;
        private static SteamStore _steamStore;

        private static bool _initialized = false;
        private static List<string> _steamApplicationDirectories;
        private static string _steamVRInstallDir;
        private static volatile bool _isAvailable;
        #endregion

        public SteamLib()
        {
            if(_initialized) return;
            Initialize();
        }


        #region Public Properties

        public static IReadOnlyDictionary<uint, string> AppInstallDirectories => _appInstallDirectories;
        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// If Steam is not installed this instance and most of its functions
        /// will be not available
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAvailable
        {
            get
            {
                if(_initialized) return _isAvailable;
                Initialize();
                return _isAvailable;
            }
        }

        /// <summary>
        ///     Gets the directory to the steam application.
        /// </summary>
        /// <value>
        ///     The steam directory.
        /// </value>
        public string SteamPath => _steamPath;

        /// <summary>
        ///     Gets the name of the steam executable file path.
        /// </summary>
        /// <value>
        ///     The full file path.
        /// </value>
        public string SteamExeFilePathName => _steamExeFilePathName;

        /// <summary>
        ///     Gets the name of the steam executable with extension.
        /// </summary>
        /// <value>
        ///     The name with extension.
        /// </value>
        public string SteamExe => _steamExe;

        /// <summary>
        ///     Gets the name of the steam process without extension.
        /// </summary>
        /// <value>
        ///     The name of the steam process.
        /// </value>
        public string SteamProcessName => _steamProcessName;

        /// <summary>
        ///     Gets the filepathname of the vr monitor executable.
        /// </summary>
        /// <value>
        ///     The full file path.
        /// </value>
        public string SteamVrInstallDir => _steamVRInstallDir;

        /// <summary>
        ///     Provides the steam application directories for the library.
        /// </summary>
        /// <value>
        ///     The steam application directories.
        /// </value>
        public List<string> SteamApplicationDirectories => _steamApplicationDirectories;
        #endregion

        #region Public Methods        
        /// <summary>
        /// Gets the application details of an Steam App by its app id from an online source.
        /// </summary>
        /// <param name="appId">The Steam AppId.</param>
        /// <param name="addImage">if set to <c>true</c> an image will be downloaded and added to the result.</param>
        /// <param name="language">The language.</param>
        /// <param name="countryCode">The country code.</param>
        /// <returns>Information about an Steam App</returns>
        public async Task<SteamAppStoreInfo> GetAppDetailsAsync(
                uint appId, bool addImage, string language = null, string countryCode = null)
        {
            // Gate every outbound request through the global rate limiter so a
            // dashboard load of 63 games doesn't blast the Steam store API with
            // 63 concurrent requests (which it answers with timeouts).
            await _appDetailsConcurrency.WaitAsync().ConfigureAwait(false);
            try
            {
                await WaitForMinimumGapAsync().ConfigureAwait(false);

                using(var httpClient = new HttpClient())
                {
                    httpClient.Timeout = WebRequestTimeout;

                    StoreAppDetailsDataModel details = null;
                    Exception lastError = null;
                    for (int attempt = 0; attempt <= AppDetailsRetryAttempts; attempt++)
                    {
                        try
                        {
                            details = await _steamStore
                                    .GetStoreAppDetailsAsync(Convert.ToInt32(appId), language, countryCode, WebRequestTimeout)
                                    .ConfigureAwait(false);
                            if(details != null) break;
                        }
                        // Anything from the HTTP layer or JSON-mapping layer is retryable.
                        // Steam occasionally returns malformed payloads under load that
                        // surface as NullReferenceException inside AutoMapper.
                        catch(Exception ex) { lastError = ex; }
                        if(attempt < AppDetailsRetryAttempts)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(400 * (attempt + 1))).ConfigureAwait(false);
                        }
                    }
                    if(details == null)
                    {
                        if(lastError != null)
                            Logger.Info($"GetAppDetailsAsync({appId}) gave up after {AppDetailsRetryAttempts + 1} attempts: {lastError.GetType().Name}: {lastError.Message}");
                        return null;
                    }
                    if(string.IsNullOrWhiteSpace(details.Name))
                    {
                        Logger.Info($"Steam store returned no Name for appid={appId} - tile will display without a title");
                    }

                    var categories = new HashSet<string>();
                    //TODO Introduce translation table for mapping between most popular steam categories and our default categories
                    if(details.Categories != null)
                    {
                        foreach(StoreCategoryModel category in details.Categories)
                        {
                            categories.Add(category.Description.ToLowerInvariant());
                        }
                    }

                    byte[] bImage = null;
                    if(addImage && !string.IsNullOrWhiteSpace(details.HeaderImage))
                    {
                        bImage = await DownloadHeaderImageAsync(appId, details.HeaderImage).ConfigureAwait(false);
                    }
                    else if(addImage)
                    {
                        Logger.Info($"Steam store returned no header_image url for appid={appId} - tile will display without a cover");
                    }

                    return new SteamAppStoreInfo(appId, bImage, categories, details);
                }
            }
            finally
            {
                _appDetailsConcurrency.Release();
            }
        }

        private static async Task WaitForMinimumGapAsync()
        {
            TimeSpan wait;
            lock(_appDetailsGate)
            {
                var sinceLast = DateTime.UtcNow - _appDetailsLastDispatchUtc;
                wait = AppDetailsMinGap - sinceLast;
                _appDetailsLastDispatchUtc = sinceLast >= AppDetailsMinGap
                    ? DateTime.UtcNow
                    : _appDetailsLastDispatchUtc + AppDetailsMinGap;
            }
            if(wait > TimeSpan.Zero) await Task.Delay(wait).ConfigureAwait(false);
        }

        private static async Task<byte[]> DownloadHeaderImageAsync(uint appId, string url)
        {
            var imageUri = new Uri(url, UriKind.Absolute);
            Exception lastError = null;
            for (int attempt = 0; attempt <= ImageDownloadRetryAttempts; attempt++)
            {
                using(var imageClient = new HttpClient { Timeout = ImageDownloadTimeout })
                {
                    try
                    {
                        return await imageClient.GetByteArrayAsync(imageUri).ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        lastError = ex;
                    }
                }
                if(attempt < ImageDownloadRetryAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1))).ConfigureAwait(false);
                }
            }
            // Image fetch failure is non-fatal - the catalog tile still appears
            // with title and categories, just without the cover.
            Logger.Info($"Header image download failed for appid={appId} from {url} after {ImageDownloadRetryAttempts + 1} attempts: {lastError?.GetType().Name}: {lastError?.Message}");
            return null;
        }

        /// <summary>
        ///     Executes the Steam Exe with the provided exeParameters.
        ///     Use it to Start an Game with paramters like: -applaunch 24234 -param1 -param2
        /// </summary>
        /// <param name="exeParameter">The parameter to execute steam with.</param>
        public SteamGame GetSteamGame(string exeParameter)
        {
            return new SteamGame(GetAppId(exeParameter), SteamExeFilePathName, exeParameter);
        }

        /// <summary>
        ///     Parse the string for possible formats of a steam Parameter/URI
        ///     that can contain an AppId.
        /// </summary>
        /// <param name="parseString">The string to parse.</param>
        /// <returns>The AppId or 0 if not found</returns>
        public int GetAppId(string parseString)
        {
            //->steam://launch/230290/vr -silent
            //->-silent -applaunch 234630 -searchds "VR-Lounge"
            //Check the format
            if(string.IsNullOrEmpty(parseString)) return 0;
            if(parseString.Contains(@"steam://"))
            {
                //URI Format provided
                var parts = parseString.Split('/');
                for(int i = 0; i < parts.Length; i++)
                {
                    if(parts[i].ToLowerInvariant().Contains("launch"))
                    {
                        int retval;
                        if(!(i + 1 < parseString.Length)) return 0;
                        if(int.TryParse(parts[i + 1].ToLowerInvariant(), out retval)) return retval;
                        return 0;
                    }
                }
            }
            else
            {
                //Applaunch Parameter provided
                //URI Format provided
                var parts = parseString.Split(' ');
                for(int i = 0; i < parts.Length; i++)
                {
                    if(parts[i].ToLowerInvariant().Contains("-applaunch"))
                    {
                        int retval;
                        if(!(i + 1 < parseString.Length)) return 0;
                        if(int.TryParse(parts[i + 1].ToLowerInvariant(), out retval)) return retval;
                        return 0;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets a list of steam applications with detailed information.
        /// This objects are mainly generated from data that is in the Steam appcache
        /// Mainly in the binary appinfo.vdf
        /// </summary>
        /// <returns>A list with information about Steam Games</returns>
        public List<SteamAppInfo> GetSteamAppInfo()
        {
            try
            {
                Logger.Debug("Started to aquire Steam Application Information");
                var retval = new List<SteamAppInfo>();
                if(!IsAvailable) return retval;
                var path = Path.Combine(SteamPath, SteamAppInfoFile);
                Logger.Debug($"AppInfo File Path is ={path}");
                if(!File.Exists(path))
                {
                    Logger.Warn($"Could not detect Steam AppInfo File at path ={path}");
                    return new List<SteamAppInfo>();
                }
                AppinfoDecoder decoder = VdfBinaryUtils.LoadBinaryVdf(path);
                foreach(KeyValuePair<string, GameData> gameData in decoder.Data)
                {
                    if(gameData.Value.Sections.TryGetValue("appinfo", out var appInfo) && appInfo is VdfData appInfoData)
                    {
                        if(appInfoData.TryGetValue("common", out var commonInfo))
                        {
                            if(commonInfo is VdfData vdfData)
                            {
                                //Requirement
                                if(vdfData.TryGetValue("type", out var appType) &&
                                   (appType.ToString() == "Game" || appType.ToString() == "Demo"))
                                {
                                    Logger.Debug("Detected a Steam Game");
                                    //Optional
                                    if(vdfData.TryGetValue("releasestate", out var releaseState) &&
                                       releaseState.ToString() != "released")
                                    {
                                        //Not installable as not released
                                        continue;
                                    }
                                    //Check further data if game is valid
                                    var game = new SteamAppInfo(appInfoData);
                                    if(game.IsValid)
                                    {
                                        retval.Add(game);
                                    }
                                    else
                                    {
                                        Logger.Warn("Game Data is not valid {game}", game.LogJson());
                                    }
                                }
                                else
                                {
                                    Logger.Trace("Non Game Application detected",vdfData.LogJson());
                                }
                            }
                            else
                            {
                                Logger.Warn("AppInfo Data is not a VdfData type!: {vdfData}",commonInfo.LogJson());
                            }
                        }
                    }
                }

                return retval;
            }
            catch(Exception e)
            {
                Logger.Error(e,"Error during parsing of AppInfo File!");
                return new List<SteamAppInfo>();
            }
            
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private static void Initialize()
        {
            try
            {
                Logger.Debug("SteamLib Initialize started...");

                _steamStore = new SteamStore();

                var steamRegistryExeFilePath = RegistryUtil.GetValueData(
                        @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                        "SteamExe");

                if(steamRegistryExeFilePath != null)
                {
                    _steamExeFilePathName = steamRegistryExeFilePath.Replace('/', '\\');
                    _steamProcessName = Path.GetFileNameWithoutExtension(_steamExeFilePathName);

                    //We dont read the SteamPath from the Registry as some games alter it and point to the wrong
                    _steamPath = new FileInfo(_steamExeFilePathName).DirectoryName;
                    Logger.Debug($"SteamLib: _steamExeFilePathName = `{_steamExeFilePathName}`");
                    Logger.Debug($"SteamLib: _steamPath = `{_steamPath}`");

                    var file = new FileInfo(_steamExeFilePathName);
                    if(file.Exists)
                    {
                        _steamExe = file.Name;
                        _steamApplicationDirectories = GetSteamLibraryFolders(_steamPath);

                        //Try to get Steam Games Installed or Installing
                        //from each steam application location
                        foreach(string directory in _steamApplicationDirectories)
                        {
                            ScanAppManifestsInLibrary(directory, _appInstallDirectories);
                        }

                        if(_appInstallDirectories.ContainsKey(SteamVRAppId))
                        {
                            _steamVRInstallDir = _appInstallDirectories[SteamVRAppId];
                        }

                        _isAvailable = true;
                    }
                    else
                    {
                        Logger.Warn(
                                "Could not detect Steam Exe in Filepath from Registry, assume Steam is not installed or corrupt");
                        _isAvailable = false;
                    }
                }
                else
                {
                    Logger.Warn("Could not find Steam Exe Filepath in registry, assume Steam is not installed");
                    _isAvailable = false;
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                _isAvailable = false;
            }

            _initialized = true;
        }

        /// <summary>
        /// Gets the steam library folders.
        /// </summary>
        /// <param name="steamPath">The path steam is installed.</param>
        /// <returns>List with installation folders for Steam</returns>
        /// <summary>
        /// Parse steamapps/libraryfolders.vdf and return every "<library>\steamapps"
        /// directory. Handles both the legacy schema (`"1" "C:\Steam\Library"`) and
        /// the post-2021 schema (`"0" { "path" "C:\Steam" ... }`), and guards every
        /// indexer + Path.Combine against null entries from a malformed or
        /// future-schema VDF - the kiosk crashed on dispatcher-unhandled
        /// `ArgumentNullException` because an entry's Value was null (modern Steam).
        /// </summary>
        internal static List<string> GetSteamLibraryFolders(string steamPath)
        {
            var retval = new List<string>();
            if (string.IsNullOrWhiteSpace(steamPath))
            {
                Logger.Warn("GetSteamLibraryFolders called with empty steamPath; returning no libraries.");
                return retval;
            }

            // Steam's own install always counts as a library.
            retval.Add(Path.Combine(steamPath, "steamapps"));

            var filePathName = Path.Combine(steamPath, "steamapps", SteamLibraryFile);
            if (!File.Exists(filePathName))
            {
                Logger.Debug($"GetSteamLibraryFolders: no libraryfolders.vdf at `{filePathName}`; only the default Steam install will be scanned.");
                return retval;
            }

            VDFFile vdfFile;
            try { vdfFile = new VDFFile(filePathName); }
            catch (Exception e)
            {
                Logger.Warn(e, $"GetSteamLibraryFolders: could not parse `{filePathName}`.");
                return retval;
            }

            foreach (KeyValuePair<string, NestedElement> pair in vdfFile.Elements)
            {
                if (pair.Value?.Children == null) continue;
                foreach (var child in pair.Value.Children)
                {
                    // Numeric keys are the library entries. The legacy schema stores
                    // the path as the entry's Value; the modern schema nests it under
                    // a "path" child.
                    if (child.Value == null) continue;
                    if (!int.TryParse(child.Key, out _)) continue;

                    string libraryRoot = ResolveLibraryPath(child.Value);
                    if (string.IsNullOrWhiteSpace(libraryRoot)) continue;

                    retval.Add(Path.Combine(libraryRoot, "steamapps"));
                }
            }

            return retval;
        }

        /// <summary>
        /// Scan one Steam-library "steamapps" directory for `appmanifest_*.acf`
        /// files and record each (appId -> common/installdir) into the supplied
        /// dictionary. Indexer-assignment is intentional - the same Steam appId
        /// can appear in multiple libraries (e.g. after moving an install), and
        /// Dictionary.Add would throw ArgumentException on the duplicate key and
        /// disable the Steam plugin for the whole kiosk session.
        /// </summary>
        internal static void ScanAppManifestsInLibrary(string steamAppsDirectory, IDictionary<uint, string> appInstallDirectories)
        {
            if (string.IsNullOrWhiteSpace(steamAppsDirectory) || appInstallDirectories == null) return;
            Logger.Debug($"SteamLib: traversing directory `{steamAppsDirectory}`...");

            var appDir = new DirectoryInfo(steamAppsDirectory);
            if (!appDir.Exists) return;

            var files = appDir.EnumerateFiles("appmanifest_*.acf");
            foreach (FileInfo info in files)
            {
                Logger.Debug($"SteamLib: traversing file `{info.FullName}`...");
                var retval = VDFSearch(info.FullName, new List<string>() { "installdir", "appid" });
                Logger.Debug($"SteamLib: traversing file `{info.FullName}` completed, retval.Count: `{retval.Count}`");
                if (retval.Count < 2) continue;
                if (uint.TryParse(retval["appid"], out var appId) &&
                    !string.IsNullOrWhiteSpace(retval["installdir"]))
                {
                    appInstallDirectories[appId] = Path.Combine(steamAppsDirectory, "common", retval["installdir"]);
                }
                else
                {
                    Logger.Warn($"Unexpected problem with ACF file={info.FullName}");
                }
            }
        }

        /// <summary>
        /// Pull the library root out of one numbered entry. Returns null if the
        /// shape doesn't match either Steam VDF schema.
        /// </summary>
        internal static string ResolveLibraryPath(NestedElement entry)
        {
            if (entry == null) return null;
            // Legacy: "1" "C:\Steam\Library"
            if (!string.IsNullOrWhiteSpace(entry.Value)) return entry.Value;
            // Modern: "0" { "path" "C:\Steam\Library" ... }
            if (entry.Children != null
                && entry.Children.TryGetValue("path", out var pathEntry)
                && !string.IsNullOrWhiteSpace(pathEntry?.Value))
            {
                return pathEntry.Value;
            }
            return null;
        }

        /// <summary>
        /// Search a VDF File (or acf) for specific keys
        /// </summary>
        /// <param name="vdfFilePathName">Name of the VDF file path.</param>
        /// <param name="searchKeys">The search keys.</param>
        /// <returns>Dictionary with the keys the search was done</returns>
        private static Dictionary<string, string> VDFSearch(string vdfFilePathName, List<string> searchKeys)
        {
            var retval = new Dictionary<string, string>();
            var vdfFile = new VDFFile(vdfFilePathName);
            foreach(KeyValuePair<string, NestedElement> pair in vdfFile.Elements)
            {
                foreach(var key in searchKeys)
                {
                    if(pair.Key.Contains(key)) retval.Add(pair.Value.Name, pair.Value.Value);
                }

                var children = pair.Value.Children.Any();
                if(children)
                {
                    foreach(KeyValuePair<string, string> child in ParseVDFChildren(pair.Value.Children, searchKeys))
                    {
                        retval.Add(child.Key, child.Value);
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// Parses the VDF children nodes for keys.
        /// </summary>
        /// <param name="pair">The node to search.</param>
        /// <param name="searchKeys">The search keys.</param>
        /// <returns>The keys found</returns>
        private static Dictionary<string, string> ParseVDFChildren(
                Dictionary<string, NestedElement> pair,
                List<string> searchKeys)
        {
            var retval = new Dictionary<string, string>();
            foreach(KeyValuePair<string, NestedElement> valuePair in pair)
            {
                foreach(var key in searchKeys)
                {
                    if(valuePair.Key.Contains(key)) retval.Add(valuePair.Key, valuePair.Value.Value);
                }

                if(valuePair.Value.Children.Any())
                {
                    foreach(
                            KeyValuePair<string, string> keyValuePair in
                            ParseVDFChildren(valuePair.Value.Children, searchKeys))
                    {
                        if(!retval.ContainsKey(keyValuePair.Key)) retval.Add(keyValuePair.Key, keyValuePair.Value);
                        else
                        {
                            Logger.Debug($"VDF Parsing double Key with Id={keyValuePair.Key}");
                        }
                    }
                }
            }

            return retval;
        }
        #endregion
    }

    /// <summary>
    /// Flags used in Steam Acf File as AppState
    /// Indicating the current Update/Install State of the Application
    /// </summary>
    [Flags]
    enum AcfStateFlags
    {
        StateInvalid = 0,
        StateUninstalled = 1,
        StateUpdateRequired = 2,
        StateFullyInstalled = 4,
        StateEncrypted = 8,
        StateLocked = 16,
        StateFilesMissing = 32,
        StateAppRunning = 64,
        StateFilesCorrupt = 128,
        StateUpdateRunning = 256,
        StateUpdatePaused = 512,
        StateUpdateStarted = 1024,
        StateUninstalling = 2048,
        StateBackupRunning = 4096,
        StateReconfiguring = 65536,
        StateValidating = 131072,
        StateAddingFiles = 262144,
        StatePreallocating = 524288,
        StateDownloading = 1048576,
        StateStaging = 2097152,
        StateCommitting = 4194304,
        StateUpdateStopping = 8388608
    }

    /// <summary>
    /// Enum used in Acf Files to specifiy the Universe the App is in
    /// </summary>
    enum AcfUniverse
    {
        IndividualUnspecified = 0,
        Public = 1,
        Beta = 2,
        Internal = 3,
        Dev = 4,
        RC = 5
    }
}