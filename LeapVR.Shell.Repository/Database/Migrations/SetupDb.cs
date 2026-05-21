using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Repository.Entities;
using LiteDB;

namespace LeapVR.Shell.Repository.Database.Migrations
{
    public static class SetupDb
    {
        #region Background Multimedia Player Playback
        //Extensions as lower invariant
        public static readonly string[] AllowedExtensions = new[]{".mp4",".webm",".mp3",".mpeg",".mkv"};
        #endregion

        public static void WriteDbInfo(string dbConnectionString)
        {
            using(var liteDatabase = new LiteDatabase(dbConnectionString))
            {
                //Set the DB Version Info
                var col = liteDatabase.GetCollection("Settings");
                col.Insert(new BsonDocument {{"Type","DBInfo"},{"DatabaseVersion", Migration.CurrentLatestVersion}});
            }
        }

        public static void Populate(IGlobalConfiguration config)
        {
            AddVideoBackground(config);

        }

        /// <summary>
        /// Adds Videos from Installation in the Media Directory as Playlist to the System 
        /// </summary>
        /// <param name="config"></param>
        private static void AddVideoBackground(IGlobalConfiguration config)
        {

            var tracks = GetMediaFiles(config);
            if(!tracks.Any())return;
            var multimediaPlaylistRepo = new MultimediaPlaylistRepository();
            var multimediaSettingsRepo = new MultimediaSettingsRepository();

            var playlist = multimediaPlaylistRepo.GetOrCreate(GlobalConfig.GetGlobalConfiguration().BackgroundPlayerId);
            foreach(Uri track in tracks)
            {
                playlist.Tracks.Add(track);
            }

            multimediaPlaylistRepo.Store(playlist);
            var playerSettings = multimediaSettingsRepo.Get(GlobalConfig.GetGlobalConfiguration().BackgroundPlayerId);
            playerSettings.AutoStart = true;
            playerSettings.Repeat = true;
            playerSettings.Store();
        }

        /// <summary>
        /// Scans the MediaDirectory for files and returns the MediaFiles as Uri
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static List<Uri> GetMediaFiles(IGlobalConfiguration config)
        {
            var mediaDirectoryFilePath = Path.Combine(config.ShellBinaryPath, GlobalConfig.GetGlobalConfiguration().MediaDirectory);
            DirectoryInfo mediaDirectoryInfo = new DirectoryInfo(mediaDirectoryFilePath);
            var tracks = new List<Uri>();
            if(!mediaDirectoryInfo.Exists)return tracks;

            foreach(FileInfo file in mediaDirectoryInfo.EnumerateFiles())
            {
                if(AllowedExtensions.Contains(file.Extension.ToLowerInvariant()))
                {
                    tracks.Add(new Uri(file.FullName));
                }
            }
            return tracks;
        }
    }
}
