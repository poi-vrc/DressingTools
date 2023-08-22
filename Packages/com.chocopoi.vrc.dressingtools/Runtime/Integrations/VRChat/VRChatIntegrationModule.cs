/*
 * File: VRChatIntegrationModule.cs
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
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    internal class VRChatIntegrationModuleConfig : ModuleConfig
    {
    }

    [InitializeOnLoad]
    internal class VRChatIntegrationModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.integrations.vrchat";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Integration: VRChat";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => int.MaxValue;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static VRChatIntegrationModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new VRChatIntegrationModuleProvider());
        }

        public override ModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<VRChatIntegrationModuleConfig>();

        public override ModuleConfig NewModuleConfig() => new VRChatIntegrationModuleConfig();
    }
}
