using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

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

        private static readonly int TargetPreferencesVersion = 1;

        private static readonly string JsonPath = "Assets/chocopoi/DressingTools/preferences.json";

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
                    Debug.LogWarning("[DressingTools] Invalid preferences JSON detected, using default preferences instead");
                    EditorUtility.DisplayDialog("DressingTools", "Invalid preferences JSON detected, using default preferences instead", "OK");
                    return GenerateDefaultPreferences();
                }
                
                if (p.version > TargetPreferencesVersion)
                {
                    Debug.LogWarning("[DressingTools] Incompatible preferences version detected, expected version " + TargetPreferencesVersion + " but preferences file is at a newer version " + p.version + ", using default preferences file instead");
                    EditorUtility.DisplayDialog("DressingTools", "Incompatible preferences file. It is at a newer version " + p.version + " > " + TargetPreferencesVersion + ".\nUsing default preferences file instead.", "OK");
                    return GenerateDefaultPreferences();
                }
                //TODO: do migration if our version is newer

                return p;
            }
            catch (IOException e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("DressingTools", "Unable to load saved preferences, using default preferences instead:\n" + e.Message, "OK");
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
                EditorUtility.DisplayDialog("DressingTools", "Unable to save preferences:\n" + e.Message, "OK");
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
            json.app.update_branch = "stable";
        }
    }
}
