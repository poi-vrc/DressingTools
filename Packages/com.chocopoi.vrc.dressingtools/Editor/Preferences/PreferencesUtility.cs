/*
 * File: PreferencesUtility.cs
 * Project: DressingTools
 * Created Date: Friday, August 11th 2023, 12:11:55 am
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

using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class PreferencesUtility
    {
        private static Localization.I18n t = Localization.I18n.Instance;

        private static readonly int TargetPreferencesVersion = 3;

        private static readonly string EditorPrefsKey = "Chocopoi.DressingTools.Preferences";

        private static readonly string DefaultUpdateBranch = "stable";

        private static Preferences _preferences = null;

        public static Preferences GetPreferences()
        {
            if (_preferences == null)
            {
                _preferences = LoadPreferences();
            }
            return _preferences;
        }

        public static Preferences LoadPreferences()
        {
            if (!EditorPrefs.HasKey(EditorPrefsKey))
            {
                Debug.Log("[DressingTools] Preferences file not found, using default preferences instead.");
                return GenerateDefaultPreferences();
            }

            try
            {
                var json = EditorPrefs.GetString(EditorPrefsKey);
                var p = JsonConvert.DeserializeObject<Preferences>(json);

                if (p == null)
                {
                    Debug.LogWarning("[DressingTools] Invalid preferences detected, using default preferences instead");
                    EditorUtility.DisplayDialog(t._("tool.name"), t._("preferences.dialog.msg.invalidPrefsUsingDefault"), t._("common.dialog.btn.ok"));
                    return GenerateDefaultPreferences();
                }

                var version = p.version;

                if (version > TargetPreferencesVersion)
                {
                    Debug.LogWarning("[DressingTools] Incompatible preferences version detected, expected version " + TargetPreferencesVersion + " but preferences file is at a newer version " + version + ", using default preferences file instead");
                    EditorUtility.DisplayDialog(t._("tool.name"), t._("preferences.dialog.msg.incompatiblePrefsVersionUsingDefault", version, TargetPreferencesVersion), t._("common.dialog.btn.ok"));
                    return GenerateDefaultPreferences();
                }
                //TODO: do migration if our version is newer

                return p;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog(t._("tool.name"), t._("preferences.dialog.msg.unableToLoadPrefsUsingDefault", e.Message), t._("common.dialog.btn.ok"));
                return GenerateDefaultPreferences();
            }
        }

        public static void SavePreferences()
        {
            try
            {
                EditorPrefs.SetString(EditorPrefsKey, JsonConvert.SerializeObject(_preferences));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog(t._("tool.name"), t._("preferences.dialog.msg.unableToSavePreferences", e.Message), t._("common.dialog.btn.ok"));
            }
        }

        public static Preferences GenerateDefaultPreferences()
        {
            var p = new Preferences();
            ResetToDefaults(p);
            return p;
        }

        public static void ResetToDefaults(Preferences p)
        {
            // Manifest version
            p.version = TargetPreferencesVersion;

            // App preferences
            if (p.app == null)
            {
                p.app = new Preferences.App();
            }
            p.app.selectedLanguage = "en";
            p.app.updateBranch = DefaultUpdateBranch;
        }
    }
}
