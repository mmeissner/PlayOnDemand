#region Licence
/****************************************************************
 *  Filename: SteamApiDataAcquisition.cs
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
#region Licence

/****************************************************************
 *  Filename: SteamApiDataAcquisition.cs
 *  ---------- ---------- ---------- ---------- ----------
 *  Author  RadoslawMedryk
 *  Date    2017-5-18
 *  Copyright (c) VSpace Tech Dev Ltd. , 2017
 *
 * This unpublished material is proprietary to VSpace Tech Dev Ltd.
 * All rights reserved. The methods and
 * techniques described herein are considered trade secrets
 * and/or confidential. Reproduction or distribution, in whole
 * or in part, is forbidden except by express written permission
 * of VSpace Tech Dev Ltd.
 ****************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using NLog;
using Steam.Models.SteamStore;
using SteamWebAPI2.Interfaces;

namespace LeapVR.Content.Creator.Logic
{
    public class SteamApiDataAcquisition : IDisposable
    {
        #region Properties & Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string SteamId { get; }
        public string Language { get; }
        public string CountryCode { get; private set; }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }

        public bool HasEnded { get; private set; }
        public Exception Exception { get; private set; }

        private readonly ReplaySubject<Empty> _whenEndedSubject;
        public IObservable<Empty> WhenEnded { get; }

        private IEnumerable<string> _countryCodes;

        private Task _acquireLogicTask;
        private readonly WebClient _webClient;
        private readonly SteamStore _steamStore;
        private readonly CancellationTokenSource _ctSource = new CancellationTokenSource();

        private readonly SemaphoreSlim _finishedSemaphore;
        public Task WhenFinishedTask { get; }

        #endregion Properties & Fields

        #region Constructors

        internal SteamApiDataAcquisition(string steamId, string language, IEnumerable<string> countryCodes)
        {
            _logger.Debug($"SteamApiDataAcquisition .ctor ENTER");

            QuickLeap.AssertNotNullEx(countryCodes);

            SteamId = steamId;
            Language = language;
            _countryCodes = countryCodes;

            _webClient = new WebClient();
            _steamStore = new SteamStore();

            _whenEndedSubject = new ReplaySubject<Empty>();
            WhenEnded = _whenEndedSubject.AsObservable();

            _finishedSemaphore = new SemaphoreSlim(0);
            WhenFinishedTask = _finishedSemaphore.WaitAsync();

            _logger.Debug($"SteamApiDataAcquisition .ctor EXIT");
        }

        #endregion Constructors

        #region Methods

        public static SteamApiDataAcquisition Acquire(string steamId, string language, IEnumerable<string> countryCodes)
        {
            _logger.Debug($"SteamApiDataAcquisition Acquire ENTER");

            var acq = new SteamApiDataAcquisition(steamId, language, countryCodes);
            acq.AcquireDataBegin();

            _logger.Debug($"SteamApiDataAcquisition Acquire EXIT");
            return acq;
        }

        public void Cancel()
        {
            _logger.Debug($"SteamApiDataAcquisition Cancel ENTER");

            _ctSource.Cancel();
            _steamStore.Cancel();

            _logger.Debug($"SteamApiDataAcquisition Cancel EXIT");
        }

        internal void AcquireDataBegin()
        {
            _logger.Debug($"SteamApiDataAcquisition AcquireDataBegin ENTER");

            _acquireLogicTask = AcquireDataLogicAsync(_ctSource.Token)
                .ContinueWith(OnAcquireLogicTaskEnded);

            _logger.Debug($"SteamApiDataAcquisition AcquireDataBegin EXIT");
        }

        private async Task AcquireDataLogicAsync(CancellationToken ct)
        {
            _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync ENTER");

            if (string.IsNullOrWhiteSpace(SteamId))
            {
                _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync EXIT1");
                return;
            }

            ct.ThrowIfCancellationRequested();

            StoreAppDetailsDataModel details = null;

            if (!_countryCodes.Any())
            {
                _countryCodes = new string[] {null};
            }
            foreach (var cc in _countryCodes)
            {
                _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync LoopForEachCC: START");
                ct.ThrowIfCancellationRequested();
                try
                {
                    details = await _steamStore.GetStoreAppDetailsAsync(int.Parse(SteamId), Language, cc);
                    _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync LoopForEachCC: DETAILS GOT: `{details}`");
                    if (details != null)
                    {
                        CountryCode = cc;
                        break;
                    }
                }
                catch (Exception e)
                {
                    _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync LoopForEachCC: EXCEPTION: `{e}`");
                    // swallow
                }
            }

            if (details == null)
            {
                _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync EXIT2");
                return;
            }

            Title = details.Name;
            Description = details.AboutTheGame;

            var downloadedDir = Path.Combine(Path.GetTempPath(), "VrLeapContentCreator", "WebDownload");
            Directory.CreateDirectory(downloadedDir);
            var downloadedFilePath = Path.Combine(downloadedDir, SteamId);

            ct.ThrowIfCancellationRequested();
            _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync BEFORE-DOWNLOAD");
            await _webClient.DownloadFileTaskAsync(details.HeaderImage, downloadedFilePath);
            _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync AFTER-DOWNLOAD");
            ImagePath = downloadedFilePath;

            _logger.Debug($"SteamApiDataAcquisition AcquireDataLogicAsync EXIT3");
        }

        private void OnAcquireLogicTaskEnded(Task task)
        {
            _logger.Debug($"SteamApiDataAcquisition OnAcquireLogicTaskEnded ENTER");
            try
            {
                _webClient.Dispose();
            }
            catch { /* swallow */ }

            Exception = task.Exception;
            HasEnded = true;

            _finishedSemaphore.Release();
            _whenEndedSubject.OnNext(Empty.Get);
            _whenEndedSubject.OnCompleted();

            _logger.Debug($"SteamApiDataAcquisition OnAcquireLogicTaskEnded EXIT");
        }

        #endregion Methods

        public void Dispose()
        {
            _whenEndedSubject?.Dispose();
            _acquireLogicTask?.Dispose();
            _webClient?.Dispose();
            _ctSource?.Dispose();
            _finishedSemaphore?.Dispose();
            WhenFinishedTask?.Dispose();
        }
    }
}
