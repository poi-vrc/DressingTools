using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser
{
    public class DTDresserSettings
    {
        [JsonIgnore]
        public GameObject targetAvatar;

        [JsonIgnore]
        public GameObject targetWearable;

        [JsonIgnore]
        public string avatarArmatureName;

        [JsonIgnore]
        public string wearableArmatureName;

        public DTDresserSettings()
        {
            // default settings
            avatarArmatureName = "Armature";
            wearableArmatureName = "Armature";
        }

        public virtual bool DrawEditorGUI()
        {
            // draws the editor GUI and returns whether it is modified or not
            var newWearableArmatureName = EditorGUILayout.DelayedTextField("Wearable Armature Name", wearableArmatureName);

            var modified = wearableArmatureName != newWearableArmatureName;
            wearableArmatureName = newWearableArmatureName;

            return modified;
        }
    }
}
