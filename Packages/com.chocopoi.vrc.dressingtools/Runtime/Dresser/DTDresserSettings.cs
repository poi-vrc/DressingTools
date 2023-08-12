/*
 * File: DTDresserSettings.cs
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

#if UNITY_EDITOR
        public virtual bool DrawEditorGUI()
        {
            // draws the editor GUI and returns whether it is modified or not
            var newWearableArmatureName = EditorGUILayout.DelayedTextField("Wearable Armature Name", wearableArmatureName);

            var modified = wearableArmatureName != newWearableArmatureName;
            wearableArmatureName = newWearableArmatureName;

            return modified;
        }
#endif
    }
}
