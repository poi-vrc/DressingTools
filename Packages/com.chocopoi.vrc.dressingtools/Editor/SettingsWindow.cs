using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class SettingsWindow : EditorWindow
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        private Vector2 scrollPos;

        private Preferences preferences;

        private int lastSelectedLanguage;

        private bool needRepaint;

        private string[] availableLocales;

        private readonly string[] availableLocalesHumanReadable;

        public SettingsWindow()
        {
            ReloadPreferences();

            availableLocales = t.GetAvailableLocales();
            availableLocalesHumanReadable = new string[availableLocales.Length];
            for (var i = 0; i < availableLocales.Length; i++)
            {
                availableLocalesHumanReadable[i] = new CultureInfo(availableLocales[i]).NativeName;
            }
        }

        private void DrawSelectLanguageGUI()
        {
            var selectedLanguage = EditorGUILayout.Popup("Language 言語:", FindArrayItemIndex(availableLocales, preferences.app.selectedLanguage), availableLocalesHumanReadable);
            if (selectedLanguage != lastSelectedLanguage)
            {
                t.SetLocale(preferences.app.selectedLanguage = availableLocales[selectedLanguage]);
                PreferencesUtility.SavePreferences();

                lastSelectedLanguage = selectedLanguage;
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
                var branchIndex = FindArrayItemIndex(branches, preferences.app.updateBranch);
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
                var selectedUpdateBranch = EditorGUILayout.Popup(t._("popup_settings_updater_current_branch"), branchIndex, branches);

                // detect changes and save to file
                if (branchIndex != selectedUpdateBranch)
                {
                    preferences.app.updateBranch = branches[selectedUpdateBranch];
                    PreferencesUtility.SavePreferences();
                }

                EditorGUILayout.LabelField(t._("label_settings_updater_current_version", DressingToolsUpdater.GetCurrentVersion()?.fullVersionString));

                var latestVersion = DressingToolsUpdater.GetBranchLatestVersion(preferences.app.updateBranch);
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

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawSelectLanguageGUI();

            EditorGUILayout.Separator();

            DTEditorUtils.DrawHorizontalLine();

            DrawUpdateBranchSelectorGUI();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(t._("button_settings_updater_reset_to_defaults")))
            {
                PreferencesUtility.ResetToDefaults(preferences);
                PreferencesUtility.SavePreferences();
                t.SetLocale(preferences.app.selectedLanguage);
            }
        }
    }
}
