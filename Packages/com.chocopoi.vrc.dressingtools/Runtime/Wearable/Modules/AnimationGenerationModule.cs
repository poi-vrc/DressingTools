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
    public class AnimationGenerationModule : DTWearableModuleBase
    {
        public static class MessageCode
        {
        }

        private const string LogLabel = "AnimationGenerationModule";

        public override int ApplyOrder => 4;

        public override bool AllowMultiple => false;

        public DTAnimationPreset avatarAnimationOnWear; // execute on wear

        public DTAnimationPreset wearableAnimationOnWear;

        public List<DTWearableCustomizable> wearableCustomizables; // items that show up in action menu for customization

        public AnimationGenerationModule()
        {
            avatarAnimationOnWear = new DTAnimationPreset();
            wearableAnimationOnWear = new DTAnimationPreset();
            wearableCustomizables = new List<DTWearableCustomizable>();
        }

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
            return true;
        }
    }
}
