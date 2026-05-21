#region Licence
/****************************************************************
 *  Filename: LanguageSelector.cs
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
using System.Globalization;
using System.Linq;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using NLog;

namespace LeapVR.Shell.Language
{
    public class LanguageSelector : ILanguageSelector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private readonly IConfigFileRepository<SystemConfig> _configFileRepository;
        public LanguageSelector(IUIMessageBroker messageBroker, IConfigFileRepository<SystemConfig> systemConfigrepo)
        {

            _messageBroker = messageBroker;
            _configFileRepository = systemConfigrepo;
            var systemConfig = _configFileRepository.Get();
            DefaultCulture = new CultureInfo(systemConfig.DefaultLanguage);
            CurrentCulture = DefaultCulture;
            SupportedCultures = (from lang in systemConfig.SupportedLanguageCultureNames select new CultureInfo(lang)).
                    ToArray();
            SetCultureInfoInternal(CurrentCulture);
        }
        public CultureInfo DefaultCulture { get; private set; }
        public CultureInfo[] SupportedCultures { get; }
        public CultureInfo CurrentCulture { get; private set; }
        public void ActivateDefaultCultureInfo() { ActivateCultureInfo(DefaultCulture); }
        public void ActivateCultureInfo(string cultureShortName) { ActivateCultureInfo(new CultureInfo(cultureShortName)); }
        public void ActivateCultureInfo(CultureInfo newCulture) { SetCultureInfoInternal(newCulture); }
        public bool SetDefaultCulture(CultureInfo culture)
        {
            try
            {
                if (!SupportedCultures.Contains(culture)) return false;
                var config = _configFileRepository.Get();
                config.DefaultLanguage = culture.Name;
                _configFileRepository.Store(config);
                DefaultCulture = culture;
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e,$"Error during attempt to save {typeof(SystemConfig)}");
                return false;
            }
        }
        void SetCultureInfoInternal(CultureInfo newCulture)
        {
            var cultureToSet = !SupportedCultures.Contains(newCulture) ? DefaultCulture : newCulture;
            CultureInfo.CurrentCulture = cultureToSet;
            CultureInfo.CurrentUICulture = cultureToSet;
            CurrentCulture = cultureToSet;
            ChangeLanguage(cultureToSet);
            _messageBroker.Publish(new UILanguageChangedEvent(cultureToSet));
        }
        private void ChangeLanguage(CultureInfo newCultureInfo)
        {
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = newCultureInfo;
        }
    }
}
