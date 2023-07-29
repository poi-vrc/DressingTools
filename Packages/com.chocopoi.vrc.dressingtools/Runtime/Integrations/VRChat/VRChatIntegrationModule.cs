using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    public class VRChatIntegrationModule : DTWearableModuleBase
    {
        public static class MessageCode
        {
            public const string IgnoredNoVRCSDK = "modules.vrchatIntegrationModule.msgCode.warn.ignoredNoVRCSDK";
        }

        private const string LogLabel = "VRChatIntegrationModule";

        public override int ApplyOrder => int.MaxValue;

        public override bool AllowMultiple => false;

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
#if !VRC_SDK_VRCSDK3
            report.LogWarnLocalized(LogLabel, MessageCode.IgnoredNoVRCSDK);
#endif
            return true;
        }
    }
}
