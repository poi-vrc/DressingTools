using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using Chocopoi.DressingTools.Logging;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default.Hooks
{
    public class ObjectPlacementHookTest : DTTestBase
    {
        private bool EvaluateHook(GameObject avatarRoot, GameObject wearableRoot, out DTReport report)
        {
            report = new DTReport();
            var settings = new DTDefaultDresserSettings();
            var boneMappings = new List<DTBoneMapping>();
            var hook = new ObjectPlacementHook();

            settings.targetAvatar = avatarRoot;
            settings.targetWearable = wearableRoot;
            settings.avatarArmatureName = "Armature";
            settings.wearableArmatureName = "Armature";
            settings.dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;

            return hook.Evaluate(report, settings, boneMappings);
        }

        private GameObject CreateRootWithArmatureAndHipsBone(string name)
        {
            var root = CreateGameObject(name);
            var armature = CreateGameObject("Armature", root.transform);
            CreateGameObject("Hips", armature.transform);
            return root;
        }

        [Test]
        public void AvatarInWearable_ReturnsCorrectErrorCode()
        {
            var avatarRoot = CreateRootWithArmatureAndHipsBone("Avatar");
            var wearableRoot = CreateRootWithArmatureAndHipsBone("Wearable");

            avatarRoot.transform.parent = wearableRoot.transform;

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.AvatarInsideWearable));
        }

        [Test]
        public void WearableInAvatar_ReturnsCorrectErrorCode()
        {
            var avatarRoot = CreateRootWithArmatureAndHipsBone("Avatar");
            var wearableRoot = CreateRootWithArmatureAndHipsBone("Wearable");

            wearableRoot.transform.parent = avatarRoot.transform;

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.WearableInsideAvatar));
        }
    }
}
