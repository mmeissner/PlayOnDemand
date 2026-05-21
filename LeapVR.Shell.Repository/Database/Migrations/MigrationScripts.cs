using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Disk;
using LiteDB;
using NLog;
using Logger = NLog.Logger;

namespace LeapVR.Shell.Repository.Database.Migrations
{
    public static class MigrationScripts
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int Migrate1To2(LiteDatabase liteDatabase)
        {
            try
            {
                //Set General Data
                var vboxPluginGuid = Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3");

                //Database Settings Migration
                if(liteDatabase.RenameCollection("DatabaseDetailsDb", "Settings"))
                {
                    var settingsCollection = liteDatabase.GetCollection("Settings");

                    //Delete all existing Docs
                    settingsCollection.Delete(x => true);
                }

                //AppPlatformData Migration
                var collectionAppPlatformData = liteDatabase.GetCollection("AppPlatformDataDb");
                var collectionInstallationData = liteDatabase.GetCollection("AppInstallationDataDb");
                foreach(BsonDocument platformDoc in collectionAppPlatformData.FindAll())
                {
                    //Execution Logic Migration
                    if(platformDoc.TryGetValue("ExecutionLogicInstructions", out var logicInstructions))
                    {
                        foreach(BsonValue logicInstruction in logicInstructions.AsArray)
                        {
                            var logicInstructionDoc = logicInstruction.AsDocument;
                            if(logicInstructionDoc != null)
                            {
                                //Change Type of MonitorInstructions to Db type
                                //Rename Relative Path to Path
                                //Add DiskEntityType
                                logicInstructionDoc["_type"] =
                                        "LeapVR.Shell.Repository.Entities.ProcessExecutionLogicDb, LeapVR.Shell.Repository";
                                if(logicInstructionDoc.TryGetValue("ExecutionFile", out var executionFilesBsonValue))
                                {
                                    var executionFilesBsonDoc = executionFilesBsonValue.AsDocument;
                                    executionFilesBsonDoc["_type"] =
                                            "LeapVR.Shell.Repository.Entities.DiskEntityDb, LeapVR.Shell.Repository";
                                    var relativePath = executionFilesBsonDoc["RelativePath"];
                                    executionFilesBsonDoc.Remove("RelativePath");
                                    executionFilesBsonDoc.Add("Path", relativePath);
                                    executionFilesBsonDoc.Add("Type", DiskEntityType.Relative.ToString());
                                }

                                //Change Type of MonitorInstructions to Db type
                                if(logicInstructionDoc.TryGetValue(
                                        "MonitorInstructions",
                                        out var monitorInstructionsBsonValue))
                                {
                                    var monitorInstructionsBsonArray = monitorInstructionsBsonValue.AsArray;
                                    foreach(var monitorInstructionBsonValue in monitorInstructionsBsonArray)
                                    {
                                        var monitorInstructionBsonDoc = monitorInstructionBsonValue.AsDocument;
                                        monitorInstructionBsonDoc["_type"] =
                                                "LeapVR.Shell.Repository.Entities.ProcessMonitorInstructionDb, LeapVR.Shell.Repository";
                                    }
                                }

                                //Add Execution Guids if non existend
                                if(!logicInstructionDoc.ContainsKey("ExecutionGuid"))
                                {
                                    logicInstructionDoc.Add("ExecutionGuid", Guid.NewGuid());
                                }
                            }
                        }
                    }

                    //Update Enabled Value
                    if(platformDoc.TryGetValue("ApplicationGuid", out var value))
                    {
                        var installationDoc = collectionInstallationData.FindOne(Query.EQ("ApplicationGuid", value));
                        if(installationDoc != null)
                        {
                            //Remove is Enabled from Installation and Move to Platform
                            if(installationDoc.TryGetValue("IsEnabled", out var isEnabled))
                            {
                                installationDoc.Remove("IsEnabled");
                                platformDoc.Add("IsEnabled", isEnabled);
                            }

                            //Add to Installation the PlatformPluginId
                            if(platformDoc.TryGetValue("PlatformPluginId", out var pluginGuid))
                            {
                                installationDoc.Add("PlatformPluginGuid", pluginGuid);
                            }

                            collectionInstallationData.Update(installationDoc);
                        }
                    }

                    collectionAppPlatformData.Update(platformDoc);
                }

                //AppDisplayData Migration
                var guidDisplayNameDict = new Dictionary<Guid, string>();
                var collectionAppDisplayData = liteDatabase.GetCollection("AppDisplayDataDb");
                foreach(BsonDocument appDisplayDataDoc in collectionAppDisplayData.FindAll())
                {
                    //Try to get DisplayName & Guid to extend AppInstallationDataDb
                    if(appDisplayDataDoc.TryGetValue("ApplicationGuid", out var appGuid))
                    {
                        if(appDisplayDataDoc.TryGetValue("Name", out var displayName))
                        {
                            guidDisplayNameDict.Add(appGuid.AsGuid, displayName.AsString);
                        }
                        else
                        {
                            guidDisplayNameDict.Add(appGuid.AsGuid, "Unknown");
                        }
                    }

                    //Change Type MainPicture to Db Type
                    //Rename RelativePath to Path
                    //Add DiskEntityType to MainPicture
                    if(appDisplayDataDoc.TryGetValue("MainPicture", out var mainPictureBsonValue))
                    {
                        var mainPictureBsonDoc = mainPictureBsonValue.AsDocument;
                        if(mainPictureBsonDoc != null)
                        {
                            mainPictureBsonDoc["_type"] =
                                    "LeapVR.Shell.Repository.Entities.DiskEntityDb, LeapVR.Shell.Repository";
                            var relativePath = mainPictureBsonDoc["RelativePath"];
                            mainPictureBsonDoc.Remove("RelativePath");
                            mainPictureBsonDoc.Add("Path", relativePath);
                            mainPictureBsonDoc.Add("Type", DiskEntityType.Relative.ToString());
                        }
                    }

                    collectionAppDisplayData.Update(appDisplayDataDoc);
                }


                //AppDisplayData Migration
                var collectionAppInstallationData = liteDatabase.GetCollection("AppInstallationDataDb");
                foreach(BsonDocument installationDoc in collectionAppInstallationData.FindAll())
                {
                    //Add DisplayName Info to InstallationDoc
                    if(installationDoc.TryGetValue("ApplicationGuid", out var appGuid))
                    {
                        if(guidDisplayNameDict.TryGetValue(appGuid, out var displayName))
                        {
                            installationDoc.Add("DisplayName", displayName);
                        }
                        else
                        {
                            installationDoc.Add("DisplayName", "Unknown");
                        }
                    }

                    //Add Plugin Info to Installation all V1 Apps are VBox Container Package installed
                    installationDoc.Add("PlatformPluginGuid", vboxPluginGuid);
                    installationDoc.Add("Type", AppInstallationType.Container.ToString());

                    collectionAppInstallationData.Update(installationDoc);
                }

                //AppStatisticData Migration
                var collectionAppStatisticData = liteDatabase.GetCollection("AppStatisticsDataDb");
                foreach(BsonDocument statisticDoc in collectionAppStatisticData.FindAll())
                {
                    //Add DisplayName Info to statisticDoc
                    if(statisticDoc.TryGetValue("ApplicationGuid", out var appGuid))
                    {
                        if(guidDisplayNameDict.TryGetValue(appGuid, out var displayName))
                        {
                            statisticDoc.Add("DisplayName", displayName);
                        }
                        else
                        {
                            statisticDoc.Add("DisplayName", "Unknown");
                        }
                    }

                    //Add Plugin Info to statistic
                    statisticDoc.Add("PlatformGuid", vboxPluginGuid);

                    collectionAppStatisticData.Update(statisticDoc);
                }

                //Adding Background Video Playback that was newly introduced
                var tracks = SetupDb.GetMediaFiles(GlobalConfig.GetGlobalConfiguration());
                if(tracks.Any())
                {
                    #region Adding the Playlist
                    var trackList = new BsonArray();
                    foreach(Uri track in tracks)
                    {
                        trackList.Add(new BsonValue(track.ToString()));
                    }

                    var newPlaylist = new BsonDocument
                                      {
                                              ["_id"] = Guid.NewGuid(),
                                              ["Identifier"] = "LoginView_Background_Player",
                                              ["Tracks"] = trackList
                                      };
                    var collectionMultimediaPlaylist = liteDatabase.GetCollection("MultimediaPlaylistDataDb");
                    var docId= collectionMultimediaPlaylist.Insert(newPlaylist);
                    Logger.Debug($"Added new Playlist with Id={docId} and {trackList.Count} Track(s)!");
                    #endregion

                    #region Adding the Player Settings
                    var collectionPlayerSettings = liteDatabase.GetCollection("MultimediaSettingsDataDb");
                    var newSettings = new BsonDocument
                                      {
                                              ["_id"] = Guid.NewGuid(),
                                              ["Identifier"] = "LoginView_Background_Player",
                                              ["AutoStart"] = new BsonValue(true),
                                              ["Repeat"] = new BsonValue(true),
                                              ["Volume"] = new BsonValue(1d)
                                      };
                    var settingsId = collectionPlayerSettings.Insert(newSettings);
                    Logger.Debug($"Added new Player Settings with Id={settingsId}");
                    #endregion

                }
                return 2;
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }
    }
}