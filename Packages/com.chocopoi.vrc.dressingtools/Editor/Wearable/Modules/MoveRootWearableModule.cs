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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json.Linq;
using Serilog;
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
        public class MessageCode
        {
            public const string AvatarPathEmpty = "modules.wearable.moveRoot.msgCode.error.avatarPathEmpty";
            public const string AvatarPathNotFound = "modules.wearable.moveRoot.msgCode.error.avatarPathNotFound";
        }
        private const string LogLabel = "MoveRootWearableModule";

        private static readonly Localization.I18n t = Localization.I18n.Instance;
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.wearable.move-root";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.moveRoot.friendlyName");
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

        public override bool OnApplyWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            return ProcessModule(cabCtx, wearCtx, modules);
        }

        public override bool OnPreviewWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            return ProcessModule(cabCtx, wearCtx, modules);
        }

        private static bool ProcessModule(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0) return true;

            var mrm = (MoveRootWearableModuleConfig)modules[0].config;

            if (mrm.avatarPath == null || mrm.avatarPath == "")
            {
                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.AvatarPathEmpty);
                return false;
            }

            // find avatar object
            var avatarObj = cabCtx.avatarGameObject.transform.Find(mrm.avatarPath);

            if (avatarObj == null)
            {
                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.AvatarPathNotFound);
                return false;
            }

            // set to parent
            wearCtx.wearableGameObject.transform.SetParent(avatarObj);

            return true;
        }
    }
}
