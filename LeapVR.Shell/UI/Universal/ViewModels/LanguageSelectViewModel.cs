#region Licence
/****************************************************************
 *  Filename: LanguageSelectViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-22
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using NLog;
using Screen = Caliburn.Micro.Screen;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class LanguageSelectViewModel : InputControllerScreen, IHandle<IUILanguageChangedEvent>, IDisposable
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ILanguageSelector _languageSelector;
        private CultureInfoViewModel _currentCulture;
        private BindableCollection<CultureInfoViewModel> _supportedCultures;
        private readonly IUIMessageBroker _messageBroker;

        public CultureInfoViewModel CurrentCulture
        {
            get => _currentCulture;
            set
            {
                _currentCulture = value;
                NotifyOfPropertyChange(() => CurrentCulture);
                _languageSelector?.ActivateCultureInfo(_currentCulture?.CultureInfo);
            }
        }
        public BindableCollection<CultureInfoViewModel> SupportedCultures
        {
            get => _supportedCultures;
            set
            {
                _supportedCultures = value;
                NotifyOfPropertyChange(() => SupportedCultures);
            }
        }
        #endregion

        #region Constructors
        public LanguageSelectViewModel(IUIMessageBroker messageBroker, ILanguageSelector languageSelector, IViewInputHandler viewInputHandler):base(viewInputHandler)
        {
            QuickLeap.AssertNotNull(messageBroker, languageSelector);
            _languageSelector = languageSelector;
            _supportedCultures = new BindableCollection<CultureInfoViewModel>(from culture in _languageSelector.SupportedCultures select new CultureInfoViewModel(culture));
            _currentCulture = GetCultureInfoViewModel(_languageSelector.CurrentCulture);

            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);


            AddControllerInputAction(ControllerInput.NextTwo, NavigateToNext);
            AddControllerInputAction(ControllerInput.PreviousTwo, NavigateToPrevious);
        }
        #endregion

        #region Methods
        public void NavigateToPrevious()
        {
            var currentCategoryIndex = SupportedCultures.IndexOf(CurrentCulture);
            if (currentCategoryIndex <= 0)
            {
                CurrentCulture = SupportedCultures.LastOrDefault();
                return;
            }
            CurrentCulture = SupportedCultures[currentCategoryIndex - 1];
        }

        public void NavigateToNext()
        {
            var currentCategoryIndex = SupportedCultures.IndexOf(CurrentCulture);
            if (currentCategoryIndex >= SupportedCultures.Count - 1)
            {
                CurrentCulture = SupportedCultures.FirstOrDefault();
                return;
            }
            CurrentCulture = SupportedCultures[currentCategoryIndex + 1];
        }

        public void Handle(IUILanguageChangedEvent cultureChangedEvent)
        {
            OnCultureInfoChanged(cultureChangedEvent.NewCultureInfo);
        }

        public new void Dispose()
        {
            _messageBroker.Unsubscribe(this);
            base.Dispose();
        }

        private void OnCultureInfoChanged(CultureInfo cultureInfo)
        {
            // [INFO]: set current culture info without calling Set method.
            _currentCulture = GetCultureInfoViewModel(cultureInfo);
            NotifyOfPropertyChange(() => CurrentCulture);
        }

        private CultureInfoViewModel GetCultureInfoViewModel(CultureInfo cultureInfo)
        {
            var requestedCultureViewModel = SupportedCultures?.FirstOrDefault(cul => cul.CultureInfo.Name.Equals(cultureInfo.Name));
            if (requestedCultureViewModel == null) requestedCultureViewModel = SupportedCultures?.First();
            return requestedCultureViewModel;
        }
        #endregion

    }
}
