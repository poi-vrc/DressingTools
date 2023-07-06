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

        [Test]
        public void AvatarInWearable_ReturnsCorrectErrorCode()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            avatarRoot.transform.parent = wearableRoot.transform;

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.AvatarInsideWearable));
        }

        [Test]
        public void WearableInAvatar_ReturnsCorrectErrorCode()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            wearableRoot.transform.parent = avatarRoot.transform;

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.WearableInsideAvatar));
        }
    }
}
