using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Lib.Dresser;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Wearable;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default
{
    public class DefaultDresserTest : DTEditorTestBase
    {
        [Test]
        public void NotDTDefaultDresserSettings_ReturnsCorrectErrorCode()
        {
            var dresser = new DefaultDresser();
            var report = dresser.Execute(new DresserSettings(), out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DefaultDresser.MessageCode.NotDefaultSettingsSettings));
        }

        [Test]
        public void NullTargetAvatar_ReturnsCorrectErrorCode()
        {
            var wearableRoot = CreateGameObject("Wearable");

            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = null,
                targetWearable = wearableRoot
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        [Test]
        public void NullTargetWearable_ReturnsCorrectErrorCode()
        {
            var avatarRoot = CreateGameObject("Avatar");

            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = null
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        private DTReport EvaluateDresser(GameObject avatarRoot, GameObject wearableRoot, out List<BoneMapping> boneMappings)
        {
            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = wearableRoot,
                avatarArmatureName = "Armature",
                wearableArmatureName = "Armature",
                dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
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
            Assert.True(report.HasLogCodeByType(DTReportLogType.Error, DefaultDresser.MessageCode.HookHasErrors));
        }

        [Test]
        public void NewSettingsTest()
        {
            var dresser = new DefaultDresser();
            Assert.NotNull(dresser.NewSettings());
        }
    }
}
