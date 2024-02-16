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

using System.Collections.ObjectModel;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
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

    internal abstract class CabinetModuleProvider : ModuleProvider
    {
        /// <summary>
        /// Invoke this hook
        /// </summary>
        /// <param name="cabCtx">Apply cabinet context</param>
        /// <param name="modules">Associated cabinet modules</param>
        /// <param name="isPreview">Whether this is a preview apply</param>
        /// <returns>Return false to stop continuing execution</returns>
        public abstract bool Invoke(CabinetContext cabCtx, ReadOnlyCollection<CabinetModule> modules, bool isPreview);

        public override bool Invoke(Context ctx)
        {
            var cabCtx = ctx.Extra<CabinetContext>();
            return Invoke(cabCtx, new ReadOnlyCollection<CabinetModule>(cabCtx.cabinetConfig.FindModules(Identifier)), false);
        }
    }

    internal abstract class WearableModuleProvider : ModuleProvider
    {
        /// <summary>
        /// Invoke this hook
        /// </summary>
        /// <param name="cabCtx">Apply cabinet context</param>
        /// <param name="wearCtx">Apply wearable context</param>
        /// <param name="modules">Associated wearable modules</param>
        /// <param name="isPreview">Whether this is a preview apply</param>
        /// <returns>Return false to stop continuing execution</returns>
        public abstract bool Invoke(CabinetContext cabCtx, WearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview);

        public override bool Invoke(Context ctx)
        {
            var cabCtx = ctx.Extra<CabinetContext>();
            foreach (var wearCtx in cabCtx.wearableContexts.Values)
            {
                if (!Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(wearCtx.wearableConfig.FindModules(Identifier)), false))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
