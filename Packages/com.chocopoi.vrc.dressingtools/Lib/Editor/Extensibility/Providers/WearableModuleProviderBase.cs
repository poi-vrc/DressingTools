/*
 * File: WearableModuleProviderBase.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 24th 2023, 15:31:11 am
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

using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Extensibility.Providers
{
    public abstract class WearableModuleProviderBase : ModuleProviderBase
    {
        public virtual bool OnBeforeApplyCabinet(ApplyCabinetContext ctx) => true;
        public virtual bool OnAfterApplyCabinet(ApplyCabinetContext ctx) => true;
        public virtual bool OnApplyWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, WearableModule moduleConfig) => true;
        public virtual bool OnAddWearableToCabinet(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject, WearableModule module) => true;
    }
}
