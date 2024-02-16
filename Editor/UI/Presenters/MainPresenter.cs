/*
 * File: MainPresenter.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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

using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;

#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#elif UNITY_2018_3_OR_NEWER
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#else
#error The current Unity version does not support PrefabStage.
#endif

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter
    {
        private const string GithubReleasesTagUrlPrefix = "https://github.com/poi-vrc/DressingTools/releases/tag/";

        private IMainView _view;

        public MainPresenter(IMainView view)
        {
            _view = view;

            // set locale before anything
            var prefs = PreferencesUtility.GetPreferences();
            I18nManager.Instance.SetLocale(prefs.app.selectedLanguage);

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.MouseMove += OnMouseMove;
            _view.UpdateAvailableUpdateButtonClick += OnUpdateAvailableUpdateButtonClick;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _view.PrefabStageOpened += OnPrefabStageOpened;
            _view.PrefabStageClosing += OnPrefabStageClosing;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.MouseMove -= OnMouseMove;
            _view.UpdateAvailableUpdateButtonClick -= OnUpdateAvailableUpdateButtonClick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                _view.ShowExitPlayModeHelpbox = true;
                _view.Repaint();
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                _view.ShowExitPlayModeHelpbox = false;
                _view.Repaint();
            }
        }

        private void OnUpdateAvailableUpdateButtonClick()
        {
            var version = UpdateChecker.LatestVersion.fullString;
            _view.OpenUrl(GithubReleasesTagUrlPrefix + version);
        }

        private void OnMouseMove()
        {
            // a dummy way to check update periodically
            if (!UpdateChecker.IsUpdateChecked())
            {
                // invalidate the info in our view
                _view.UpdateAvailableFromVersion = null;
                _view.UpdateAvailableToVersion = null;
                _view.Repaint();
            }

            if (UpdateChecker.IsUpdateAvailable())
            {
                _view.UpdateAvailableFromVersion = UpdateChecker.CurrentVersion.fullString;
                _view.UpdateAvailableToVersion = UpdateChecker.LatestVersion.fullString;
                _view.Repaint();
            }
        }

        private void OnPrefabStageOpened(PrefabStage stage)
        {
            _view.ShowExitPrefabModeHelpbox = true;
            _view.Repaint();
        }

        private void OnPrefabStageClosing(PrefabStage stage)
        {
            _view.ShowExitPrefabModeHelpbox = false;
            _view.Repaint();
        }

        private void OnLoad()
        {
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
