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
    /// Blendshape sync wearable module config
    /// </summary>
    internal class BlendshapeSyncWearableModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.built-in.wearable.blendshape-sync";

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Blendshape sync
        /// </summary>
        [Serializable]
        public class BlendshapeSync
        {
            /// <summary>
            /// Path to avatar SMR object relative to avatar root
            /// </summary>
            public string avatarPath;

            /// <summary>
            /// Avatar SMR blendshape name to sync
            /// </summary>
            public string avatarBlendshapeName;

            /// <summary>
            /// Avatar blendshape from value (This API is not implemented and subject to change, you should write it as 0 for now)
            /// </summary>
            public float avatarFromValue;

            /// <summary>
            /// Avatar blendshape to value (This API is not implemented and subject to change, you should write it as 100 for now)
            /// </summary>
            public float avatarToValue;

            /// <summary>
            /// Path to wearable SMR object relative to wearable root
            /// </summary>
            public string wearablePath;

            /// <summary>
            /// Wearable SMR blendshape name to sync
            /// </summary>
            public string wearableBlendshapeName;

            /// <summary>
            /// Wearable blendshape from value (This API is not implemented and subject to change, you should write it as 0 for now)
            /// </summary>
            public float wearableFromValue;

            /// <summary>
            /// Wearable blendshape to value (This API is not implemented and subject to change, you should write it as 100 for now)
            /// </summary>
            public float wearableToValue;
        }

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Blendshape sync list
        /// </summary>
        public List<BlendshapeSync> blendshapeSyncs;

        /// <summary>
        /// Construct new blendshape sync wearable module config
        /// </summary>
        public BlendshapeSyncWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            blendshapeSyncs = new List<BlendshapeSync>();
        }
    }
}
