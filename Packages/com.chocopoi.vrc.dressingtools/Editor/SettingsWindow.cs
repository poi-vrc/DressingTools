using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Chocopoi.DressingTools
{
    public class SettingsWindow : EditorWindow
    {
        private static Translation.I18n t = Translation.I18n.GetInstance();

        private Vector2 scrollPos;

        private Preferences preferences;

        private bool needRepaint;

        public SettingsWindow()
        {
            ReloadPreferences();
        }

        private void DrawSelectLanguageGUI()
        {
            GUILayout.Label("Language 語言 言語:");

            int selectedLanguage = GUILayout.Toolbar(preferences.app.selectedLanguage, new string[] { "English", "中文", "日本語", "한국어", "Français" });
            if (selectedLanguage != preferences.app.selectedLanguage)
            {
                preferences.app.selectedLanguage = selectedLanguage;
                UpdateLocale();
                PreferencesUtility.SavePreferences();
            }
        }

        private int FindArrayItemIndex(string[] array, string item)
        {
            for (int i = 0; i < array.Length; i++)
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
                string[] branches = DressingToolsUpdater.GetAvailableBranches();
                int branchIndex = FindArrayItemIndex(branches, preferences.app.updateBranch);
                if (branchIndex == -1)
                {
                    GUILayout.Label(t._("label_settings_updater_current_update_branch_cannot_be_found_switching_to_default", preferences.app.updateBranch), EditorStyles.boldLabel);

                    branchIndex = FindArrayItemIndex(branches, DressingToolsUpdater.GetDefaultBranchName());
                    if (branchIndex == -1)
                    {
                        branchIndex = 0;
                        GUILayout.Label(t._("label_settings_updater_default_branch_cannot_be_found_switching_to_first_branch"), EditorStyles.boldLabel);
                    }
                }
                int selectedUpdateBranch = EditorGUILayout.Popup(t._("popup_settings_updater_current_branch"), branchIndex, branches);

                // detect changes and save to file
                if (branchIndex != selectedUpdateBranch)
                {
                    preferences.app.updateBranch = branches[selectedUpdateBranch];
                    PreferencesUtility.SavePreferences();
                }

                EditorGUILayout.LabelField(t._("label_settings_updater_current_version", DressingToolsUpdater.GetCurrentVersion().full_version_string));

                DressingToolsUpdater.ManifestBranch latestVersion = DressingToolsUpdater.GetBranchLatestVersion(preferences.app.updateBranch);
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
            if (needRepaint)
            {
                needRepaint = false;
                Repaint();
            }
        }

        private void FinishFetchOnlineVersion(DressingToolsUpdater.Manifest manifest)
        {
            needRepaint = true;
        }

        private void ReloadPreferences()
        {
            preferences = PreferencesUtility.GetPreferences();
        }

        private void UpdateLocale()
        {
            if (preferences.app.selectedLanguage == 0)
            {
                t.SetLocale("en-gb");
            }
            else if (preferences.app.selectedLanguage == 1)
            {
                t.SetLocale("zh-tw");
            }
            else if (preferences.app.selectedLanguage == 2)
            {
                t.SetLocale("jp");
            }
            else if (preferences.app.selectedLanguage == 3)
            {
                t.SetLocale("kor");
            }
            else if (preferences.app.selectedLanguage == 4)
            {
                t.SetLocale("fr");
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawSelectLanguageGUI();

            EditorGUILayout.Separator();

            DressingUtils.DrawHorizontalLine();

            DrawUpdateBranchSelectorGUI();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(t._("button_settings_updater_reset_to_defaults")))
            {
                PreferencesUtility.ResetToDefaults(preferences);
                PreferencesUtility.SavePreferences();
                UpdateLocale();
            }
        }
    }
}
