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

using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    internal class VRChatIntegrationModule : WearableModuleBase
    {
        public static class MessageCode
        {
            public const string IgnoredNoVRCSDK = "modules.vrchatIntegrationModule.msgCode.warn.ignoredNoVRCSDK";
        }

        private const string LogLabel = "VRChatIntegrationModule";

        public override int ApplyOrder => int.MaxValue;

        public override bool AllowMultiple => false;

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, WearableConfig config, GameObject wearableGameObject)
        {
#if !VRC_SDK_VRCSDK3
            report.LogWarnLocalized(LogLabel, MessageCode.IgnoredNoVRCSDK);
#endif
            return true;
        }
    }
}
