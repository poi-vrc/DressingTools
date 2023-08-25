/*
 * File: AnimationGenerationCabinetModule.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Wearable;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Cabinet.Modules
{
    internal class AnimationGenerationCabinetModuleConfig : IModuleConfig
    {
        public Dictionary<string, AnimationPreset> savedAvatarPresets;
        public Dictionary<string, AnimationPreset> savedWearablePresets;

        public AnimationGenerationCabinetModuleConfig()
        {
            savedAvatarPresets = new Dictionary<string, AnimationPreset>();
            savedWearablePresets = new Dictionary<string, AnimationPreset>();
        }
    }

    [InitializeOnLoad]
    internal class AnimationGenerationCabinetModuleProvider : CabinetModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.cabinet.animation-generation";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Animation Generation";
        [ExcludeFromCodeCoverage] public override int CallOrder => 4;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static AnimationGenerationCabinetModuleProvider()
        {
            CabinetModuleProviderLocator.Instance.Register(new AnimationGenerationCabinetModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<AnimationGenerationCabinetModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new AnimationGenerationCabinetModuleConfig();
    }
}
