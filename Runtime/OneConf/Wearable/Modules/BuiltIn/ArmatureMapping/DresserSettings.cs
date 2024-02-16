/*
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

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping
{
    /// <summary>
    /// Dresser settings (This API will be changed soon)
    /// </summary>
    internal class DresserSettings
    {
        /// <summary>
        /// Target avatar
        /// </summary>
        [JsonIgnore]
        public GameObject targetAvatar;

        /// <summary>
        /// Target wearable
        /// </summary>
        [JsonIgnore]
        public GameObject targetWearable;

        /// <summary>
        /// Avatar armature name
        /// </summary>
        [JsonIgnore]
        public string avatarArmatureName;

        /// <summary>
        /// Wearable armature name
        /// </summary>
        [JsonIgnore]
        public string wearableArmatureName;

        /// <summary>
        /// Constructs a new dresser settings
        /// </summary>
        public DresserSettings()
        {
            // default settings
            avatarArmatureName = "Armature";
            wearableArmatureName = "Armature";
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw editor IMGUI (This API will be changed soon)
        /// </summary>
        /// <returns>Mark as modified</returns>
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
