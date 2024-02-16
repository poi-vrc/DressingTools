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
using System.Globalization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;

namespace Chocopoi.DressingTools.OneConf.Wearable
{
    /// <summary>
    /// Wearable configuration
    /// </summary>
    internal class WearableConfig
    {
        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Wearable meta information
        /// </summary>
        public WearableInfo info { get; set; }

        /// <summary>
        /// Avatar configuration
        /// </summary>
        public AvatarConfig avatarConfig { get; set; }

        /// <summary>
        /// Modules
        /// </summary>
        public List<WearableModule> modules;

        /// <summary>
        /// Constructs a new wearable configuration
        /// </summary>
        public WearableConfig()
        {
            // initialize some fields
            version = CurrentConfigVersion;
            var isoTimeStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            info = new WearableInfo
            {
                uuid = Guid.NewGuid().ToString(),
                createdTime = isoTimeStr,
                updatedTime = isoTimeStr
            };
            avatarConfig = new AvatarConfig();
            modules = new List<WearableModule>();
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

        public WearableModule FindModule(string moduleName)
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

        public List<WearableModule> FindModules(string moduleName)
        {
            var list = new List<WearableModule>();
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
