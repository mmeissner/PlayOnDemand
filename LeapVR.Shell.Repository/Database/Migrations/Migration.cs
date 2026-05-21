using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteDB;
using NLog;
using Logger = NLog.Logger;

namespace LeapVR.Shell.Repository.Database.Migrations
{
    
    public static class Migration
    {
        public const int CurrentLatestVersion = 2;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Migrate(string dbConnectionString)
        {
            try
            {
                using(var liteDatabase = new LiteDatabase(dbConnectionString))
                {
                    var version = GetVersion(liteDatabase);

                    //Should normally not be as there should be an version received
                    //that was set through population of db or by a previous version
                    if(version == 0)
                    {
                        Logger.Warn("Could not detect a Version information in Database! Going to set to latest version");
                        SetVersion(CurrentLatestVersion,liteDatabase);
                        return;
                    }

                    //Is Up to Date
                    if(version == CurrentLatestVersion) return;

                    //Needs Migration
                    int lastVersionMigratedTo;
                    for(int i = version + 1; i < CurrentLatestVersion + 1; i++)
                    {
                        lastVersionMigratedTo = Update(i, liteDatabase);
                        if(lastVersionMigratedTo == CurrentLatestVersion) break;
                        if(lastVersionMigratedTo == 0)
                        {
                            throw new NotImplementedException(
                                    $"There was an error during the Migration from Version={version} to Version={i}!");
                        }
                    }

                    //Save Update
                    SetVersion(CurrentLatestVersion,liteDatabase);
                }
            }
            catch(System.Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private static int GetVersion(LiteDatabase liteDatabase)
        {
            BsonDocument versionDoc = null;
            int version = 0;

            //Legacy
            var col = liteDatabase.GetCollection("DatabaseDetailsDb");

            //Try New Format
            if(col.Count() == 0)
            {
                //Latest Info Store
                col = liteDatabase.GetCollection("Settings");
                if(col.Count() == 0)
                {
                    throw new NotSupportedException("This Database is not supported by this version: No DBVersion Info found");
                }
                else
                {
                    versionDoc = col.Find(x=> x.Contains(new KeyValuePair<string, BsonValue>("Type","DBInfo"))).FirstOrDefault();
                    //Version Doc in Collection
                    if(versionDoc != null)
                    {
                        if(versionDoc.TryGetValue("DatabaseVersion",out var versionBsonValue))
                        {
                            version = versionBsonValue.AsInt32;
                        }
                    }
                    //No Version Doc in Collection
                    return version;
                }
            }

            //Is Legacy Format
            versionDoc = col.Find(x=> true).FirstOrDefault();
            if(versionDoc != null)
            {
                if(versionDoc.TryGetValue("Version", out var versionBsonValue))
                {
                    version = versionBsonValue.AsInt32;
                }
            }

            return version;
        }

        private static void SetVersion(int newVersion,LiteDatabase liteDatabase)
        {
            var col = liteDatabase.GetCollection("Settings");
            BsonDocument versionDoc = null;
            versionDoc = col.Find(x=> x.Contains(new KeyValuePair<string, BsonValue>("Type","DBInfo"))).FirstOrDefault();

            //Update Value in document
            if(versionDoc != null)
            {
                if(versionDoc.ContainsKey("DatabaseVersion"))
                {
                    versionDoc["DatabaseVersion"] = newVersion;
                }
                else
                {
                    versionDoc.Add("DatabaseVersion",newVersion);
                }

                col.Update(versionDoc);
            }

            //Create Document and Value
            else
            {
                col.Insert(new BsonDocument {{"Type","DBInfo"},{"DatabaseVersion", newVersion}});
            }
        }
        private static int Update(int nextVersion, LiteDatabase liteDatabase)
        {
            switch(nextVersion)
            {
                //From 1 to 2
                case 2:
                    return MigrationScripts.Migrate1To2(liteDatabase);
                default:
                    return nextVersion;
            }
        }
    }
}