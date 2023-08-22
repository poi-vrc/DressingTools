/*
 * File: AnimationGenerationModule.cs
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
    internal class AnimationGenerationModuleConfig : ModuleConfig
    {
        public AnimationPreset avatarAnimationOnWear; // execute on wear
        public AnimationPreset wearableAnimationOnWear;
        public List<WearableCustomizable> wearableCustomizables; // items that show up in action menu for customization

        public AnimationGenerationModuleConfig()
        {
            avatarAnimationOnWear = new AnimationPreset();
            wearableAnimationOnWear = new AnimationPreset();
            wearableCustomizables = new List<WearableCustomizable>();
        }
    }

    [InitializeOnLoad]
    internal class AnimationGenerationModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.built-in.animation-generation";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Animation Generation";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => 4;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static AnimationGenerationModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new AnimationGenerationModuleProvider());
        }

        public override ModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<AnimationGenerationModuleConfig>();

        public override ModuleConfig NewModuleConfig() => new AnimationGenerationModuleConfig();
    }

}
