using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    public sealed class UnknownModule : DTWearableModuleBase
    {
        public sealed override int ApplyOrder => int.MaxValue;

        public override bool AllowMultiple => true;

        public readonly string moduleTypeName;

        public readonly string rawJson;

        public UnknownModule(string moduleTypeName, string rawJson)
        {
            this.moduleTypeName = moduleTypeName;
            this.rawJson = rawJson;
        }

        public sealed override string Serialize()
        {
            return rawJson;
        }

        public sealed override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
            return true;
        }
    }
}
