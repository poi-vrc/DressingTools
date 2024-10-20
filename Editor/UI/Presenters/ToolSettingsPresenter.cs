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
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class ToolSettingsPresenter
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private IToolSettingsSubView _view;
        private Preferences _prefs;

        public ToolSettingsPresenter(IToolSettingsSubView view)
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
            _view.UpdaterCheckUpdateButtonClicked += OnUpdaterCheckUpdateButtonClicked;
            _view.ResetToDefaultsButtonClicked += OnResetToDefaultsButtonClicked;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
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
            UpdateChecker.InvalidateVersionCheckCache();
            var _ = UpdateChecker.LatestVersion;
            UpdateView();
        }

        private void UpdateUpdateCheckerView()
        {
            _view.UpdaterCurrentVersion = UpdateChecker.CurrentVersion?.fullString;
            _view.UpdaterShowHelpboxUpdateNotChecked = !UpdateChecker.IsUpdateChecked();
        }

        private void UpdateView()
        {
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
