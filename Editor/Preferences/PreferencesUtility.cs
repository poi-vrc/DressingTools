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

using System;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class PreferencesUtility
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private static readonly string EditorPrefsKey = "Chocopoi.DressingTools.Preferences";

        private static Preferences s_prefs = null;

        internal static IUI ui = new UnityEditorUI();

        public static Preferences GetPreferences()
        {
            s_prefs ??= LoadPreferences();
            return s_prefs;
        }

        public static Preferences LoadPreferences()
        {
            if (!EditorPrefs.HasKey(EditorPrefsKey))
            {
                Debug.Log("[DressingTools] Preferences not found, using default preferences instead.");
                return new Preferences();
            }

            try
            {
                var json = EditorPrefs.GetString(EditorPrefsKey);

                // TODO: do schema check

                var jObject = JObject.Parse(json);

                if (jObject == null)
                {
                    Debug.LogWarning("[DressingTools] Invalid preferences detected, using default preferences instead");
                    ui.ShowInvalidPrefsUsingDefaultDialog();
                    return new Preferences();
                }

                var version = jObject["version"].ToObject<SerializationVersion>();
                if (version.Major > Preferences.CurrentConfigVersion.Major)
                {
                    Debug.LogWarning("[DressingTools] Incompatible preferences version detected, expected version " + Preferences.CurrentConfigVersion + " but preferences file is at a newer version " + version + ", using default preferences file instead");
                    ui.ShowIncompatiblePrefsVersionUsingDefaultDialog(version.ToString());
                    return new Preferences();
                }

                // TODO: do migration if needed

                return jObject.ToObject<Preferences>();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                ui.ShowUnableToLoadPrefsVersionUsingDefaultDialog(e);
                return new Preferences();
            }
        }

        public static void SavePreferences()
        {
            try
            {
                EditorPrefs.SetString(EditorPrefsKey, JsonConvert.SerializeObject(s_prefs));
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                ui.ShowUnableToSavePrefsVersionUsingDefaultDialog(e);
            }
        }

        public static void ResetToDefaults()
        {
            s_prefs.ResetToDefaults();
            SavePreferences();
        }

        // interface for mocking in unit tests
        internal interface IUI
        {
            void ShowInvalidPrefsUsingDefaultDialog();
            void ShowIncompatiblePrefsVersionUsingDefaultDialog(string version);
            void ShowUnableToLoadPrefsVersionUsingDefaultDialog(Exception e);
            void ShowUnableToSavePrefsVersionUsingDefaultDialog(Exception e);
        }

        internal class UnityEditorUI : IUI
        {
            public void ShowIncompatiblePrefsVersionUsingDefaultDialog(string version)
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("settings.dialog.msg.incompatiblePrefsVersionUsingDefault", version, Preferences.CurrentConfigVersion), t._("common.dialog.btn.ok"));
            }

            public void ShowInvalidPrefsUsingDefaultDialog()
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("settings.dialog.msg.invalidPrefsUsingDefault"), t._("common.dialog.btn.ok"));
            }

            public void ShowUnableToLoadPrefsVersionUsingDefaultDialog(Exception e)
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("settings.dialog.msg.unableToLoadPrefsUsingDefault", e.Message), t._("common.dialog.btn.ok"));
            }

            public void ShowUnableToSavePrefsVersionUsingDefaultDialog(Exception e)
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("settings.dialog.msg.unableToSavePreferences", e.Message), t._("common.dialog.btn.ok"));
            }
        }
    }
}
