using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using Chocopoi.DressingTools.Logging;
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
            dresser.Execute(new DTDresserSettings(), out var report);
            Assert.AreEqual(report.Result, DTReportResult.InvalidSettings);
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
            dresser.Execute(settings, out var report);
            Assert.AreEqual(report.Result, DTReportResult.InvalidSettings);
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
            dresser.Execute(settings, out var report);
            Assert.AreEqual(report.Result, DTReportResult.InvalidSettings);
        }

        private DTBoneMapping[] EvaluateDresser(GameObject avatarRoot, GameObject wearableRoot, out DTReport report)
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
            return dresser.Execute(settings, out report);
        }

        [Test]
        public void AbortOnHookFalse()
        {
            // we create roots with no armature to simulate an error
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");
            Assert.Null(EvaluateDresser(avatarRoot, wearableRoot, out var report));
            Assert.AreEqual(report.Result, DTReportResult.Incompatible);
        }

        [Test]
        public void SetResultToCompatibleIfReportHasWarnings()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            // an extra object in armature introduces the warnings
            CreateGameObject("MyObject", avatarArmature.transform);

            Assert.NotNull(EvaluateDresser(avatarRoot, wearableRoot, out var report));
            Assert.AreEqual(report.Result, DTReportResult.Compatible);
        }

        [Test]
        public void SetResultToOkIfReportHasNoWarnings()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            Assert.NotNull(EvaluateDresser(avatarRoot, wearableRoot, out var report));
            Assert.AreEqual(report.Result, DTReportResult.Ok);
        }
    }
}
