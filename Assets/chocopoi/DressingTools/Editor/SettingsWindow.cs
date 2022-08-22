using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Chocopoi.DressingTools.Reporting;

namespace Chocopoi.DressingTools
{
    public class SettingsWindow : EditorWindow
    {
        private static Translation.I18n t = Translation.I18n.GetInstance();

        private Vector2 scrollPos;

        private Preferences.Json preferences;

        private bool needRepaint;

        public SettingsWindow()
        {
            ReloadPreferences();
        }

        private void DrawSelectLanguageGUI()
        {
            GUILayout.Label("Language 語言 言語:");

            int selectedLanguage = GUILayout.Toolbar(preferences.app.selected_language, new string[] { "English", "中文", "日本語", "한국어", "Français" });
            if (selectedLanguage != preferences.app.selected_language)
            {
                preferences.app.selected_language = selectedLanguage;
                UpdateLocale();
                Preferences.SavePreferences();
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
            GUILayout.Label("Update Checker", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            if (DressingToolsUpdater.IsUpdateChecked() && !DressingToolsUpdater.IsLastUpdateCheckErrored())
            {
                EditorGUILayout.LabelField("Default branch: " + DressingToolsUpdater.GetDefaultBranchName());

                // get the branches from manifest and display in GUI
                string[] branches = DressingToolsUpdater.GetAvailableBranches();
                int branchIndex = FindArrayItemIndex(branches, preferences.app.update_branch);
                if (branchIndex == -1)
                {
                    GUILayout.Label("The current branch \"" + preferences.app.update_branch + "\" cannot be found.\nChanging to default branch instead.", EditorStyles.boldLabel);

                    branchIndex = FindArrayItemIndex(branches, DressingToolsUpdater.GetDefaultBranchName());
                    if (branchIndex == -1)
                    {
                        branchIndex = 0;
                        GUILayout.Label("Default branch cannot be found.\nChanging to branch 0 instead.", EditorStyles.boldLabel);
                    }
                }
                int selectedUpdateBranch = EditorGUILayout.Popup("Current branch:", branchIndex, branches);

                // detect changes and save to file
                if (branchIndex != selectedUpdateBranch)
                {
                    preferences.app.update_branch = branches[selectedUpdateBranch];
                    Preferences.SavePreferences();
                }

                EditorGUILayout.LabelField("Current version: " + DressingToolsUpdater.GetCurrentVersion().full_version_string);

                DressingToolsUpdater.ManifestBranch latestVersion = DressingToolsUpdater.GetBranchLatestVersion(preferences.app.update_branch);
                EditorGUILayout.LabelField("Latest version: " + latestVersion.version);

                EditorGUILayout.LabelField("Status: " + (DressingToolsUpdater.IsUpdateAvailable() ? "Update Available" : "Up-to-date"));
            } else
            {
                if (DressingToolsUpdater.IsLastUpdateCheckErrored())
                {
                    GUILayout.Label("The last manifest download has failed.\nPlease retry to change the update branch:");
                } else
                {
                    GUILayout.Label("The manifest has not been downloaded.\nPlease check update to change the update branch:");
                }
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Check Update"))
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
            preferences = Preferences.GetPreferences();
        }

        private void UpdateLocale()
        {
            if (preferences.app.selected_language == 0)
            {
                t.SetLocale("en");
            }
            else if (preferences.app.selected_language == 1)
            {
                t.SetLocale("zh");
            }
            else if (preferences.app.selected_language == 2)
            {
                t.SetLocale("jp");
            }
            else if (preferences.app.selected_language == 3)
            {
                t.SetLocale("kr");
            }
            else if (preferences.app.selected_language == 4)
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
            
            if (GUILayout.Button("Reset to defaults"))
            {
                Preferences.ResetToDefaults(preferences);
                Preferences.SavePreferences();
                UpdateLocale();
            }
        }
    }
}