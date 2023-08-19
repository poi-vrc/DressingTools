/*
 * File: SettingsWindow.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
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

using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class SettingsWindow : EditorWindow
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        private Vector2 _scrollPos;

        private Preferences _preferences;

        private int _lastSelectedLanguage;

        private bool _needRepaint;

        private string[] _availableLocales;

        private readonly string[] _availableLocalesHumanReadable;

        public SettingsWindow()
        {
            ReloadPreferences();

            _availableLocales = t.GetAvailableLocales();
            _availableLocalesHumanReadable = new string[_availableLocales.Length];
            for (var i = 0; i < _availableLocales.Length; i++)
            {
                _availableLocalesHumanReadable[i] = new CultureInfo(_availableLocales[i]).NativeName;
            }
        }

        private void DrawSelectLanguageGUI()
        {
            var selectedLanguage = EditorGUILayout.Popup("Language 言語:", FindArrayItemIndex(_availableLocales, _preferences.app.selectedLanguage), _availableLocalesHumanReadable);
            if (selectedLanguage != _lastSelectedLanguage)
            {
                t.SetLocale(_preferences.app.selectedLanguage = _availableLocales[selectedLanguage]);
                PreferencesUtility.SavePreferences();

                _lastSelectedLanguage = selectedLanguage;
            }
        }

        private int FindArrayItemIndex(string[] array, string item)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        private void DrawUpdateBranchSelectorGUI()
        {
            GUILayout.Label(t._("label_settings_updater_checker"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            if (DressingToolsUpdater.IsUpdateChecked() && !DressingToolsUpdater.IsLastUpdateCheckErrored())
            {
                EditorGUILayout.LabelField(t._("label_settings_updater_default_branch", DressingToolsUpdater.GetDefaultBranchName()));

                // get the branches from manifest and display in GUI
                var branches = DressingToolsUpdater.GetAvailableBranches();
                var branchIndex = FindArrayItemIndex(branches, _preferences.app.updateBranch);
                if (branchIndex == -1)
                {
                    GUILayout.Label(t._("label_settings_updater_current_update_branch_cannot_be_found_switching_to_default", _preferences.app.updateBranch), EditorStyles.boldLabel);

                    branchIndex = FindArrayItemIndex(branches, DressingToolsUpdater.GetDefaultBranchName());
                    if (branchIndex == -1)
                    {
                        branchIndex = 0;
                        GUILayout.Label(t._("label_settings_updater_default_branch_cannot_be_found_switching_to_first_branch"), EditorStyles.boldLabel);
                    }
                }
                var selectedUpdateBranch = EditorGUILayout.Popup(t._("popup_settings_updater_current_branch"), branchIndex, branches);

                // detect changes and save to file
                if (branchIndex != selectedUpdateBranch)
                {
                    _preferences.app.updateBranch = branches[selectedUpdateBranch];
                    PreferencesUtility.SavePreferences();
                }

                EditorGUILayout.LabelField(t._("label_settings_updater_current_version", DressingToolsUpdater.GetCurrentVersion()?.fullVersionString));

                var latestVersion = DressingToolsUpdater.GetBranchLatestVersion(_preferences.app.updateBranch);
                EditorGUILayout.LabelField(t._("label_settings_updater_latest_version", latestVersion.version));

                if (DressingToolsUpdater.IsUpdateAvailable())
                {
                    EditorGUILayout.LabelField(t._("label_settings_updater_status_update_available"));
                }
                else
                {
                    EditorGUILayout.LabelField(t._("label_settings_updater_status_up_to_date"));
                }
            }
            else
            {
                if (DressingToolsUpdater.IsLastUpdateCheckErrored())
                {
                    GUILayout.Label(t._("label_settings_updater_last_update_check_errored"));
                }
                else
                {
                    GUILayout.Label(t._("label_settings_updater_manifest_not_downloaded"));
                }
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button(t._("button_settings_updater_check_update")))
            {
                DressingToolsUpdater.FetchOnlineVersion(FinishFetchOnlineVersion);
            }
        }

        private void Update()
        {
            if (_needRepaint)
            {
                _needRepaint = false;
                Repaint();
            }
        }

        private void FinishFetchOnlineVersion(DressingToolsUpdater.Manifest manifest)
        {
            _needRepaint = true;
        }

        private void ReloadPreferences()
        {
            _preferences = PreferencesUtility.GetPreferences();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSelectLanguageGUI();

            EditorGUILayout.Separator();

            DTEditorUtils.DrawHorizontalLine();

            DrawUpdateBranchSelectorGUI();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(t._("button_settings_updater_reset_to_defaults")))
            {
                PreferencesUtility.ResetToDefaults(_preferences);
                PreferencesUtility.SavePreferences();
                t.SetLocale(_preferences.app.selectedLanguage);
            }
        }
    }
}
