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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn
{
    /// <summary>
    /// Armature mapping wearable module config
    /// </summary>
    internal class ArmatureMappingWearableModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.built-in.wearable.armature-mapping";

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Dresser class full name
        /// </summary>
        public string dresserName;

        /// <summary>
        /// Wearable armature name
        /// </summary>
        public string wearableArmatureName;

        /// <summary>
        /// Bone mapping mode
        /// </summary>
        public BoneMappingMode boneMappingMode;

        /// <summary>
        /// User-defined bone mappings. Content depends on bone mapping mode.
        /// </summary>
        public List<BoneMapping> boneMappings;

        /// <summary>
        /// Serialized dresser config JSON
        /// </summary>
        public string serializedDresserConfig;

        /// <summary>
        /// Remove existing prefixes and suffixes
        /// </summary>
        public bool removeExistingPrefixSuffix;

        /// <summary>
        /// Group bones
        /// </summary>
        public bool groupBones;

        /// <summary>
        /// Construct new armature mapping wearable module config
        /// </summary>
        public ArmatureMappingWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            dresserName = null;
            wearableArmatureName = null;
            boneMappingMode = BoneMappingMode.Auto;
            boneMappings = null;
            serializedDresserConfig = "{}";
            removeExistingPrefixSuffix = true;
            groupBones = true;
        }
    }
}
