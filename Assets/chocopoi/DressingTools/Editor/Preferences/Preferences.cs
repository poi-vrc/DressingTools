using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class Preferences
    {
        [Serializable]
        public class JsonApp
        {
            public int selected_language;
            public string update_branch;
        }

        [Serializable]
        public class Json
        {
            public int version;
            public JsonApp app;
        }

        private static Translation.I18n t = Translation.I18n.GetInstance();

        private static readonly int TargetPreferencesVersion = 1;

        private static readonly string JsonPath = "Assets/chocopoi/DressingTools/preferences.json";

        private static readonly string DefaultUpdateBranch = "stable";

        private static Json preferences = null;

        public static Json GetPreferences()
        {
            if (preferences == null)
            {
                preferences = LoadPreferences();
            }
            return preferences;
        }

        public static Json LoadPreferences()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.Log("[DressingTools] Preferences file not found, using default preferences instead.");
                return GenerateDefaultPreferences();
            }

            try
            {
                string json = File.ReadAllText(JsonPath);
                Json p = JsonUtility.FromJson<Json>(json);

                if (p == null)
                {
                    Debug.LogWarning("[DressingTools] Invalid preferences file detected, using default preferences instead");
                    EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_invalid_preferences_file"), "OK");
                    return GenerateDefaultPreferences();
                }

                if (p.version > TargetPreferencesVersion)
                {
                    Debug.LogWarning("[DressingTools] Incompatible preferences version detected, expected version " + TargetPreferencesVersion + " but preferences file is at a newer version " + p.version + ", using default preferences file instead");
                    EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_incompatible_preferences_file", p.version, TargetPreferencesVersion), "OK");
                    return GenerateDefaultPreferences();
                }
                //TODO: do migration if our version is newer

                return p;
            }
            catch (IOException e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_unable_to_load_preferences_file", e.Message), "OK");
                return GenerateDefaultPreferences();
            }
        }

        public static void SavePreferences()
        {
            try
            {
                File.WriteAllText(JsonPath, JsonUtility.ToJson(preferences));
            }
            catch (IOException e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_unable_to_save_preferences_file", e.Message), "OK");
            }
        }

        public static Json GenerateDefaultPreferences()
        {
            Json json = new Json();
            ResetToDefaults(json);
            return json;
        }

        public static void ResetToDefaults(Json json)
        {
            // Manifest version
            json.version = TargetPreferencesVersion;

            // App preferences
            if (json.app == null)
            {
                json.app = new JsonApp();
            }
            json.app.selected_language = 0;

            // attempt to get current branch version
            try
            {
                json.app.update_branch = DressingToolsUpdater.GetCurrentVersion()?.branch ?? DefaultUpdateBranch;
            }
            catch (Exception)
            {
                json.app.update_branch = DefaultUpdateBranch;
            }
        }
    }
}
