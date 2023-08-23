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
    internal class MoveRootModuleConfig : IModuleConfig
    {
        public string avatarPath;
    }

    [InitializeOnLoad]
    internal class MoveRootModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.built-in.move-root";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Move Root";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => 2;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static MoveRootModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new MoveRootModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<MoveRootModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new MoveRootModuleConfig();
    }
}
