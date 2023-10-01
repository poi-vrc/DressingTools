/*
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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Cabinet.Modules;
using Chocopoi.DressingFramework.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Cabinet.Modules
{
    [InitializeOnLoad]
    internal class CabinetAnimCabinetModuleProvider : CabinetModuleProviderBase
    {
        [ExcludeFromCodeCoverage] public override string Identifier => CabinetAnimCabinetModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Cabinet Animation";
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage]
        public override CabinetApplyConstraint Constraint => ApplyAtStage(CabinetApplyStage.Transpose, CabinetHookStageRunOrder.Before).Build();

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

        public override bool Invoke(ApplyCabinetContext cabCtx, ReadOnlyCollection<CabinetModule> modules, bool isPreview) => true;
    }
}
