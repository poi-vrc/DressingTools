/*
 * File: CabinetConfig.cs
 * Project: DressingFramework
 * Created Date: Saturday, Aug 24th 2023, 05:08:11 pm
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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Cabinet
{
    /// <summary>
    /// Cabinet config
    /// </summary>
    internal class CabinetConfig
    {
        public enum WriteDefaultsMode
        {
            Auto = 0,
            On = 1,
            Off = 2
        }

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Avatar armature name
        /// </summary>
        public string avatarArmatureName;

        /// <summary>
        /// Group dynamics
        /// </summary>
        public bool groupDynamics;

        /// <summary>
        /// Group dynamics and separate into different GameObjects
        /// </summary>
        public bool groupDynamicsSeparateGameObjects;

        /// <summary>
        /// Animation write defaults mode. Animation-related modules should respect this option
        /// </summary>
        public WriteDefaultsMode animationWriteDefaultsMode;

        /// <summary>
        /// Cabinet modules
        /// </summary>
        public List<CabinetModule> modules;

        /// <summary>
        /// Initialize a new cabinet configuration
        /// </summary>
        public CabinetConfig()
        {
            version = CurrentConfigVersion;
            avatarArmatureName = "Armature";
            groupDynamics = true;
            groupDynamicsSeparateGameObjects = true;
            animationWriteDefaultsMode = WriteDefaultsMode.Auto;
            modules = new List<CabinetModule>();
        }

        public bool IsValid()
        {
            if (version.Major > CurrentConfigVersion.Major)
            {
                Debug.LogWarning($"[DressingTools] Incompatibile cabinet config version detected: {version}");
                return false;
            }

            bool valid = true;
            valid &= !string.IsNullOrEmpty(avatarArmatureName.Trim());
            return valid;
        }

        public T FindModuleConfig<T>() where T : IModuleConfig
        {
            var list = new List<T>();
            foreach (var module in modules)
            {
                if (module.config is T moduleConfig)
                {
                    return moduleConfig;
                }
            }
            return default;
        }

        public List<T> FindModuleConfigs<T>() where T : IModuleConfig
        {
            var list = new List<T>();
            foreach (var module in modules)
            {
                if (module.config is T moduleConfig)
                {
                    list.Add(moduleConfig);
                }
            }
            return list;
        }

        public CabinetModule FindModule(string moduleName)
        {
            foreach (var module in modules)
            {
                if (module.moduleName == moduleName)
                {
                    return module;
                }
            }
            return null;
        }

        public List<CabinetModule> FindModules(string moduleName)
        {
            var list = new List<CabinetModule>();
            foreach (var module in modules)
            {
                if (module.moduleName == moduleName)
                {
                    list.Add(module);
                }
            }
            return list;
        }
    }
}
