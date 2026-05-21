#region Licence
/****************************************************************
 *  Filename: LookUpFile.cs
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
using NLog;

namespace LeapVR.Content.Util.Util
{
    public class LookUpFile
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        bool _isFileExisting;
        private bool _writeMissingToFile;
        private string _filePathName;

        public HashSet<string> Categories { get; set; } = new HashSet<string>() { "Casual", "Education", "Music", "Puzzle", "Room Escape", "Shooter", "Sport" };
        public LookUpFile(string filePathName)
        {
            _filePathName = filePathName;
            _isFileExisting = CheckLookUpFile(filePathName);
        }
        public bool WriteMissingToFile
        {
            get
            {
                if (_isFileExisting) return _writeMissingToFile;
                return false;
            }
            set
            {
                if (!_isFileExisting) {_writeMissingToFile = false; return;}
                _writeMissingToFile = value;
            }
        }
        public int LookUpAppId(string indexname)
        {
            int retval;
            if (!GetFromFile(_filePathName, indexname, out retval)) if(WriteMissingToFile) WriteMissingAppIdToFile(_filePathName, indexname);
            return retval;
        }
        public string LookUpCategory(int appId)
        {
            string retval;
            if (!GetFromFile(_filePathName, appId, out retval)) if (WriteMissingToFile) WriteMissingCategoryToFile(_filePathName, appId);
            return retval;
        }
        private bool CheckLookUpFile(string filePathName)
        {
            try
            {
                if (!File.Exists(filePathName)) File.AppendAllLines(filePathName, new[] {
                    ";-----APPID & CATEGORY LOOKUP FILE-----",
                    $"[{Library.LookUpFileAppId}]",
                    $"{Environment.NewLine}",
                    ";CATEGORIES:Casual, Education, Music, Puzzle, Room Escape, Shooter, Sport",
                    $"[{Library.LookUpFileCategory}]"});
                return File.Exists(filePathName);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }
        private bool GetFromFile(string filePathName,int appId, out string category)
        {
            category = null;
            var result = IniParser.ReadValue(Library.LookUpFileCategory, appId.ToString(), filePathName);
            if (String.IsNullOrEmpty(result))
            {
                category = null;
                return false;
            }
            var retval = Categories.FirstOrDefault(x => x.ToLowerInvariant() == result.ToLowerInvariant());
            if (retval != null)
            {
                category = retval;
                return true;
            }
            return false;
        }
        private bool GetFromFile(string filePathName,string index,out int appId)
        {
            appId = -1;
            var result = IniParser.ReadValue(Library.LookUpFileAppId, index, filePathName);
            if (String.IsNullOrEmpty(result))
            {
                return false;
            }
            
            var retval = Categories.FirstOrDefault(x => x.ToLowerInvariant() == result.ToLowerInvariant());
            if (Int32.TryParse(result,out appId))
            {
                return true;
            }
            return false;
        }

        private void WriteMissingAppIdToFile(string filePathName, string indexName)
        {
            try
            {
                IniParser.WriteValue(Library.LookUpFileAppId, indexName, "", filePathName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        private void WriteMissingCategoryToFile(string filePathName, int appId)
        {
            try
            {
                IniParser.WriteValue(Library.LookUpFileCategory, appId.ToString(), "", filePathName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }
    }
}
