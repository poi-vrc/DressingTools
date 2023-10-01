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
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingFramework.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Localization;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    [InitializeOnLoad]
    internal class MoveRootWearableModuleProvider : WearableModuleProviderBase
    {
        public class MessageCode
        {
            public const string AvatarPathEmpty = "modules.wearable.moveRoot.msgCode.error.avatarPathEmpty";
            public const string AvatarPathNotFound = "modules.wearable.moveRoot.msgCode.error.avatarPathNotFound";
        }
        private const string LogLabel = "MoveRootWearableModule";

        private static readonly I18nTranslator t = I18n.ToolTranslator;

        [ExcludeFromCodeCoverage] public override string Identifier => MoveRootWearableModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.moveRoot.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage] public override WearableApplyConstraint Constraint => ApplyAtStage(CabinetApplyStage.Transpose).Build();

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

        public override bool Invoke(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (modules.Count == 0) return true;

            var mrm = (MoveRootWearableModuleConfig)modules[0].config;

            if (mrm.avatarPath == null || mrm.avatarPath == "")
            {
                cabCtx.report.LogErrorLocalized(t, LogLabel, MessageCode.AvatarPathEmpty);
                return false;
            }

            // find avatar object
            var avatarObj = cabCtx.avatarGameObject.transform.Find(mrm.avatarPath);

            if (avatarObj == null)
            {
                cabCtx.report.LogErrorLocalized(t, LogLabel, MessageCode.AvatarPathNotFound);
                return false;
            }

            // set to parent
            wearCtx.wearableGameObject.transform.SetParent(avatarObj);

            return true;
        }
    }
}
