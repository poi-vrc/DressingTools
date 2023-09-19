/*
 * File: CabinetAnimCabinetModule.cs
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
using Chocopoi.DressingFramework.Extensibility.Providers;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.Wearable;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Cabinet.Modules
{
    internal class CabinetAnimCabinetModuleConfig : IModuleConfig
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public SerializationVersion version;
        public Dictionary<string, AnimationPreset> savedAvatarPresets;
        public Dictionary<string, AnimationPreset> savedWearablePresets;

        public CabinetAnimCabinetModuleConfig()
        {
            version = CurrentConfigVersion;
            savedAvatarPresets = new Dictionary<string, AnimationPreset>();
            savedWearablePresets = new Dictionary<string, AnimationPreset>();
        }
    }

    [InitializeOnLoad]
    internal class CabinetAnimCabinetModuleProvider : CabinetModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.cabinet.cabinet-anim";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Cabinet Animation";
        [ExcludeFromCodeCoverage] public override int CallOrder => 4;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static CabinetAnimCabinetModuleProvider()
        {
            CabinetModuleProviderLocator.Instance.Register(new CabinetAnimCabinetModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > CabinetAnimCabinetModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible CabinetAnimCabinetModule version: " + version.Major + " > " + CabinetAnimCabinetModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<CabinetAnimCabinetModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new CabinetAnimCabinetModuleConfig();
    }
}
