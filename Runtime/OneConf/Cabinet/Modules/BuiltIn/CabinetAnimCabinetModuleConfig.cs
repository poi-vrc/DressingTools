﻿/*
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
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;

namespace Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn
{
    /// <summary>
    /// Cabinet animation cabinet module config
    /// </summary>
    internal class CabinetAnimCabinetModuleConfig : IModuleConfig
    {
        public const string DefaultCabinetMenuName = "DT Cabinet";

        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.built-in.cabinet.cabinet-anim";

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 1, 0);

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Saved avatar animation presets
        /// </summary>
        public Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedAvatarPresets;

        /// <summary>
        /// Saved wearable animation presets
        /// </summary>
        public Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedWearablePresets;

        /// <summary>
        /// Enable cabinet thumbnails to use in toggle icons
        /// </summary>
        public bool thumbnails;

        /// <summary>
        /// Install path for the cabinet menu
        /// </summary>
        public string menuInstallPath;

        /// <summary>
        /// Cabinet menu item name
        /// </summary>
        public string menuItemName;

        /// <summary>
        /// Network synced
        /// </summary>
        public bool networkSynced;

        /// <summary>
        /// Saved
        /// </summary>
        public bool saved;

        /// <summary>
        /// Reset customizables when switching to another outfit
        /// </summary>
        public bool resetCustomizablesOnSwitch;

        /// <summary>
        /// Constructs a new cabinet animation cabinet module config
        /// </summary>
        public CabinetAnimCabinetModuleConfig()
        {
            version = CurrentConfigVersion;
            savedAvatarPresets = new Dictionary<string, CabinetAnimWearableModuleConfig.Preset>();
            savedWearablePresets = new Dictionary<string, CabinetAnimWearableModuleConfig.Preset>();
            thumbnails = true;
            menuInstallPath = "";
            menuItemName = DefaultCabinetMenuName;
            networkSynced = true;
            saved = true;
            resetCustomizablesOnSwitch = true;
        }
    }
}
