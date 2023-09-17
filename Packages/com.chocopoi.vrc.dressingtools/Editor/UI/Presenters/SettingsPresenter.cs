/*
 * File: SettingsPresenter.cs
 * Project: DressingTools
 * Created Date: Sunday, September 17th 2023, 9:45:36 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */


using System;
using System.Globalization;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class SettingsPresenter
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        private ISettingsSubView _view;
        private Preferences _prefs;
        private string[] _availableLocales;

        public SettingsPresenter(ISettingsSubView view)
        {
            _view = view;
            _prefs = PreferencesUtility.GetPreferences();

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.LanguageChanged += OnLanguageChanged;
            _view.SettingsChanged += OnSettingsChanged;
            _view.UpdaterCheckUpdateButtonClicked += OnUpdaterCheckUpdateButtonClicked;
            _view.ResetToDefaultsButtonClicked += OnResetToDefaultsButtonClicked;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.SettingsChanged -= OnSettingsChanged;
            _view.UpdaterCheckUpdateButtonClicked -= OnUpdaterCheckUpdateButtonClicked;
            _view.ResetToDefaultsButtonClicked -= OnResetToDefaultsButtonClicked;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnResetToDefaultsButtonClicked()
        {
            PreferencesUtility.ResetToDefaults();
            UpdateView();
        }

        private void OnUpdaterCheckUpdateButtonClicked()
        {
            UpdateChecker.FetchOnlineVersion();
            UpdateView();
        }

        private void OnLanguageChanged()
        {
            var localeIndex = _view.AvailableLanguageKeys.IndexOf(_view.LanguageSelected);
            var locale = _availableLocales[localeIndex];

            _prefs.app.selectedLanguage = locale;
            t.SetLocale(locale);
            PreferencesUtility.SavePreferences();

            _view.AskReloadWindow();
        }

        private void OnSettingsChanged()
        {
            _prefs.cabinet.defaultArmatureName = _view.CabinetDefaultsArmatureName;
            _prefs.cabinet.defaultGroupDynamics = _view.CabinetDefaultsGroupDynamics;
            _prefs.cabinet.defaultGroupDynamicsSeparateDynamics = _view.CabinetDefaultsSeparateDynamics;
            _prefs.cabinet.defaultAnimationWriteDefaults = _view.CabinetDefaultsAnimWriteDefaults;

            _prefs.app.updateBranch = _view.UpdaterSelectedBranch;

            PreferencesUtility.SavePreferences();
        }

        private void UpdateLanguagePopupView()
        {
            _availableLocales = t.GetAvailableLocales();

            _view.AvailableLanguageKeys.Clear();
            foreach (var locale in _availableLocales)
            {
                _view.AvailableLanguageKeys.Add(new CultureInfo(locale).NativeName);
            }

            var localeIndex = Array.IndexOf(_availableLocales, _prefs.app.selectedLanguage);
            if (localeIndex == -1)
            {
                localeIndex = 0;
            }
            _view.LanguageSelected = _view.AvailableLanguageKeys[localeIndex];
        }

        private void UpdateCabinetDefaultsView()
        {
            _view.CabinetDefaultsArmatureName = _prefs.cabinet.defaultArmatureName;
            _view.CabinetDefaultsGroupDynamics = _prefs.cabinet.defaultGroupDynamics;
            _view.CabinetDefaultsSeparateDynamics = _prefs.cabinet.defaultGroupDynamicsSeparateDynamics;
            _view.CabinetDefaultsAnimWriteDefaults = _prefs.cabinet.defaultAnimationWriteDefaults;
        }

        private void UpdateUpdateCheckerView()
        {
            _view.UpdaterCurrentVersion = UpdateChecker.CurrentVersion?.fullVersionString;

            if (UpdateChecker.IsUpdateChecked())
            {
                _view.UpdaterShowHelpboxUpdateNotChecked = false;
                _view.UpdaterDefaultBranch = UpdateChecker.GetDefaultBranchName();

                _view.AvailableBranchKeys.Clear();
                var branches = UpdateChecker.GetAvailableBranches();
                _view.AvailableBranchKeys.AddRange(branches);

                var branchIndex = Array.IndexOf(branches, _prefs.app.updateBranch);
                if (branchIndex == -1)
                {
                    branchIndex = 0;
                }
                _view.UpdaterSelectedBranch = branches[branchIndex];
            }
            else
            {
                _view.UpdaterShowHelpboxUpdateNotChecked = true;
            }
        }

        private void UpdateView()
        {
            UpdateLanguagePopupView();
            UpdateCabinetDefaultsView();
            UpdateUpdateCheckerView();

            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
