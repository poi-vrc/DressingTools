/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.OneConf.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.OneConf
{
    internal abstract class ModuleProvider : BuildPass
    {
        /// <summary>
        /// Allow multiple module to exist in the same configuration
        /// </summary>
        public abstract bool AllowMultiple { get; }

        /// <summary>
        /// Deserialize module configuration
        /// </summary>
        /// <param name="jObject">JObject</param>
        /// <returns>Module config</returns>
        public abstract IModuleConfig DeserializeModuleConfig(JObject jObject);

        /// <summary>
        /// Serialize module configuration
        /// </summary>
        /// <param name="moduleConfig">Module config</param>
        /// <returns>Serialized JSON string</returns>
        public virtual string SerializeModuleConfig(IModuleConfig moduleConfig) => JsonConvert.SerializeObject(moduleConfig);

        /// <summary>
        /// Create a new module configuration
        /// </summary>
        /// <returns>Empty module config</returns>
        public abstract IModuleConfig NewModuleConfig();
    }
}
