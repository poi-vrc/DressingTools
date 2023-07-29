using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    public class MoveRootModule : DTWearableModuleBase
    {
        public static class MessageCode
        {
        }

        private const string LogLabel = "MoveRootModule";

        public override int ApplyOrder => 2;

        public string avatarPath;

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
            return true;
        }
    }
}
