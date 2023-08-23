/*
 * File: BlendshapeSyncModule.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class BlendshapeSyncModuleConfig : IModuleConfig
    {
        public List<AnimationBlendshapeSync> blendshapeSyncs; // blendshapes to sync from avatar to wearables

        public BlendshapeSyncModuleConfig()
        {
            blendshapeSyncs = new List<AnimationBlendshapeSync>();
        }
    }

    [InitializeOnLoad]
    internal class BlendshapeSyncModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.built-in.blendshape-sync";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Blendshape Sync";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => 6;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static BlendshapeSyncModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new BlendshapeSyncModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<BlendshapeSyncModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new BlendshapeSyncModuleConfig();
    }
}
