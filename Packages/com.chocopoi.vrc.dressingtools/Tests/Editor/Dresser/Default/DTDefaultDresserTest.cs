using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default
{
    public class DTDefaultDresserTest : DTTestBase
    {
        [Test]
        public void NotDTDefaultDresserSettings_ReturnsCorrectErrorCode()
        {
            var dresser = new DTDefaultDresser();
            var report = dresser.Execute(new DTDresserSettings(), out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DTDefaultDresser.MessageCode.NotDefaultSettingsSettings));
        }

        [Test]
        public void NullTargetAvatar_ReturnsCorrectErrorCode()
        {
            var wearableRoot = CreateGameObject("Wearable");

            var dresser = new DTDefaultDresser();
            var settings = new DTDefaultDresserSettings()
            {
                targetAvatar = null,
                targetWearable = wearableRoot
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DTDefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        [Test]
        public void NullTargetWearable_ReturnsCorrectErrorCode()
        {
            var avatarRoot = CreateGameObject("Avatar");

            var dresser = new DTDefaultDresser();
            var settings = new DTDefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = null
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DTDefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        private DTReport EvaluateDresser(GameObject avatarRoot, GameObject wearableRoot, out List<DTBoneMapping> boneMappings)
        {
            var dresser = new DTDefaultDresser();
            var settings = new DTDefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = wearableRoot,
                avatarArmatureName = "Armature",
                wearableArmatureName = "Armature",
                dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
            };
            return dresser.Execute(settings, out boneMappings);
        }

        [Test]
        public void AbortOnHookFalse()
        {
            // we create roots with no armature to simulate an error
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");
            var report = EvaluateDresser(avatarRoot, wearableRoot, out var boneMappings);
            Assert.Null(boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DTDefaultDresser.MessageCode.HookHasErrors));
        }
    }
}
