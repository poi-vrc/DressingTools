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

using System;
using System.Collections.Generic;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn
{
    /// <summary>
    /// Cabinet animation wearable module config
    /// </summary>
    internal class CabinetAnimWearableModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.built-in.wearable.cabinet-anim";

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Toggle
        /// </summary>
        [Serializable]
        public class Toggle
        {
            /// <summary>
            /// Path to GameObject
            /// </summary>
            public string path;

            /// <summary>
            /// State
            /// </summary>
            public bool state;

            /// <summary>
            /// Creates a new toggle
            /// </summary>
            public Toggle()
            {
                path = null;
                state = false;
            }

            /// <summary>
            /// Copy from another toggle
            /// </summary>
            /// <param name="animationToggle"></param>
            public Toggle(Toggle animationToggle)
            {
                path = animationToggle.path;
                state = animationToggle.state;
            }
        }

        /// <summary>
        /// Blendshape value
        /// </summary>
        [Serializable]
        public class BlendshapeValue
        {
            /// <summary>
            /// Path to SkinnedMeshRenderer GameObject
            /// </summary>
            public string path;

            /// <summary>
            /// Blendshape name to control
            /// </summary>
            public string blendshapeName;

            /// <summary>
            /// Target blendshape value
            /// </summary>
            public float value;

            /// <summary>
            /// Creates a new blendshape value
            /// </summary>
            public BlendshapeValue()
            {
                path = null;
                blendshapeName = null;
                value = 0;
            }

            /// <summary>
            /// Copy from another blendshape value
            /// </summary>
            /// <param name="blendshapeValue">Blendshape value</param>
            public BlendshapeValue(BlendshapeValue blendshapeValue)
            {
                path = blendshapeValue.path;
                blendshapeName = blendshapeValue.blendshapeName;
                value = blendshapeValue.value;
            }
        }

        /// <summary>
        /// Animation preset
        /// </summary>
        [Serializable]
        public class Preset
        {
            /// <summary>
            /// Toggles
            /// </summary>
            public List<Toggle> toggles;

            /// <summary>
            /// Blendshapes
            /// </summary>
            public List<BlendshapeValue> blendshapes;

            /// <summary>
            /// Creates a new animation
            /// </summary>
            public Preset()
            {
                toggles = new List<Toggle>();
                blendshapes = new List<BlendshapeValue>();
            }

            /// <summary>
            ///  Copy from another preset
            /// </summary>
            /// <param name="preset">Preset</param>
            public Preset(Preset preset)
            {
                // deep copy
                toggles = preset.toggles.ConvertAll(x => new Toggle(x));
                blendshapes = preset.blendshapes.ConvertAll(x => new BlendshapeValue(x));
            }
        }

        /// <summary>
        /// Customizable type
        /// </summary>
        [Serializable]
        public enum CustomizableType
        {
            /// <summary>
            /// A toggle on/off customizable
            /// </summary>
            Toggle = 0,

            /// <summary>
            /// Blendshape customizable
            /// </summary>
            Blendshape = 1
        }

        /// <summary>
        /// Customizable
        /// </summary>
        [Serializable]
        public class Customizable
        {
            /// <summary>
            /// Name of the customizable
            /// </summary>
            public string name;

            /// <summary>
            /// Type
            /// </summary>
            public CustomizableType type;

            /// <summary>
            /// Default customizable value
            /// </summary>
            public float defaultValue;

            /// <summary>
            /// Avatar toggles (Toggle type only)
            /// </summary>
            public List<Toggle> avatarToggles;

            /// <summary>
            /// Wearable toggles (Toggle type only)
            /// </summary>
            public List<Toggle> wearableToggles;

            /// <summary>
            /// Avatar blendshapes (Toggle type only)
            /// </summary>
            public List<BlendshapeValue> avatarBlendshapes;

            /// <summary>
            /// Wearable blendshapes
            /// </summary>
            public List<BlendshapeValue> wearableBlendshapes;

            /// <summary>
            /// Constructs a new customizable
            /// </summary>
            public Customizable()
            {
                name = null;
                type = CustomizableType.Toggle;
                avatarToggles = new List<Toggle>();
                wearableToggles = new List<Toggle>();
                avatarBlendshapes = new List<BlendshapeValue>();
                wearableBlendshapes = new List<BlendshapeValue>();
            }
        }

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Avatar animation preset to play on-wear
        /// </summary>
        public Preset avatarAnimationOnWear;

        /// <summary>
        /// Wearable animation preset to play on-wear
        /// </summary>
        public Preset wearableAnimationOnWear;

        /// <summary>
        /// Customizables that show in menu (if supported)
        /// </summary>
        public List<Customizable> wearableCustomizables;

        /// <summary>
        /// Automatically set avatar toggle original states to inverted
        /// </summary>
        public bool invertAvatarToggleOriginalStates;

        /// <summary>
        /// Automatically set wearable toggle original states to inverted
        /// </summary>
        public bool invertWearableToggleOriginalStates;

        /// <summary>
        /// Automatically set wearable dynamics to inactive
        /// </summary>
        public bool setWearableDynamicsInactive;

        /// <summary>
        /// Constructs a new cabinet animation wearable module config
        /// </summary>
        public CabinetAnimWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            avatarAnimationOnWear = new Preset();
            wearableAnimationOnWear = new Preset();
            wearableCustomizables = new List<Customizable>();
            invertAvatarToggleOriginalStates = false;
            invertWearableToggleOriginalStates = true;
            setWearableDynamicsInactive = true;
        }
    }
}
