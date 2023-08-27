/*
 * File: MoveRootModule.cs
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

using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class MoveRootWearableModuleConfig : IModuleConfig
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public SerializationVersion version;
        public string avatarPath;

        public MoveRootWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            avatarPath = null;
        }
    }

    [InitializeOnLoad]
    internal class MoveRootWearableModuleProvider : WearableModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.wearable.move-root";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Move Root";
        [ExcludeFromCodeCoverage] public override int CallOrder => 2;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static MoveRootWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new MoveRootWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > MoveRootWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible MoveRootWearableModuleConfig version: " + version.Major + " > " + MoveRootWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<MoveRootWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new MoveRootWearableModuleConfig();
    }
}
