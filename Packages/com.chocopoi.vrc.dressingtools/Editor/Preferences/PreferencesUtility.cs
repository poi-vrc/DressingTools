using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class PreferencesUtility
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        private static readonly int TargetPreferencesVersion = 2;

        private static readonly string JsonPath = "Assets/chocopoi/DressingTools/Resources";

        private static readonly string JsonFileName = "preferences.json";

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
            if (!File.Exists(JsonPath))
            {
                Debug.Log("[DressingTools] Preferences file not found, using default preferences instead.");
                return GenerateDefaultPreferences();
            }

            try
            {
                var json = File.ReadAllText(JsonPath);
                var p = JsonConvert.DeserializeObject<Preferences>(json);

                if (p == null)
                {
                    Debug.LogWarning("[DressingTools] Invalid preferences file detected, using default preferences instead");
                    EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_invalid_preferences_file"), "OK");
                    return GenerateDefaultPreferences();
                }

                var version = p.version;

                if (version > TargetPreferencesVersion)
                {
                    Debug.LogWarning("[DressingTools] Incompatible preferences version detected, expected version " + TargetPreferencesVersion + " but preferences file is at a newer version " + version + ", using default preferences file instead");
                    EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_incompatible_preferences_file", version, TargetPreferencesVersion), "OK");
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
                if (!Directory.Exists(JsonPath))
                {
                    Directory.CreateDirectory(JsonPath);
                }
                File.WriteAllText(JsonPath + "/" + JsonFileName, JsonConvert.SerializeObject(_preferences));
            }
            catch (IOException e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_unable_to_save_preferences_file", e.Message), "OK");
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
