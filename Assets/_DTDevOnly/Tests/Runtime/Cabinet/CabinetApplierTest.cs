using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Wearable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class CabinetApplierTest : DTTestBase
    {
        [Test]
        public void AvatarWithOneWearable_AppliesNormally()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.Apply(report);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");
        }

        [Test]
        public void ConfigVersionTooNew_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            // we simulate this by adding the config version by one
            var wearableComp = avatarRoot.GetComponentInChildren<DTCabinetWearable>();
            Assert.NotNull(wearableComp);
            JObject json = JObject.Parse(wearableComp.configJson);
            json["configVersion"] = WearableConfig.CurrentConfigVersion + 1;
            wearableComp.configJson = json.ToString(Formatting.None);

            var report = new DTReport();
            cabinet.Apply(report);

            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.IncompatibleConfigVersion), "Should have incompatible config version error");
        }

        // TODO: write config migration test

        [Test]
        public void ConfigDeserializationFailure_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            // we simulate this by destroying the config json
            var wearableComp = avatarRoot.GetComponentInChildren<DTCabinetWearable>();
            Assert.NotNull(wearableComp);
            wearableComp.configJson = "ababababababababa";

            var report = new DTReport();
            cabinet.Apply(report);

            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.UnableToDeserializeConfig), "Should have deserialization error");
        }

        [Test]
        public void GroupDynamicsToSeparateGameObjectsCorrectly()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearableOtherDynamics.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.GroupDynamics = true;
            cabinet.GroupDynamicsSeparateGameObjects = true;
            cabinet.Apply(report);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");

            // get wearable root
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);

            // get dynamics container
            var dynamicsContainer = wearableRoot.Find("DT_Dynamics");
            Assert.NotNull(dynamicsContainer);

            // check dynamics
            var wearableDynamicsList = DTRuntimeUtils.ScanDynamics(wearableRoot.gameObject);
            foreach (var wearableDynamics in wearableDynamicsList)
            {
                Assert.AreEqual(dynamicsContainer, wearableDynamics.Transform.parent);
            }
        }

        [Test]
        public void GroupDynamicsToSingleGameObjectCorrectly()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearableOtherDynamics.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.GroupDynamics = true;
            cabinet.GroupDynamicsSeparateGameObjects = false;
            cabinet.Apply(report);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");

            // get wearable root
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);

            // get dynamics container
            var dynamicsContainer = wearableRoot.Find("DT_Dynamics");
            Assert.NotNull(dynamicsContainer);

            // check dynamics
            var wearableDynamicsList = DTRuntimeUtils.ScanDynamics(wearableRoot.gameObject);
            foreach (var wearableDynamics in wearableDynamicsList)
            {
                Assert.AreEqual(dynamicsContainer, wearableDynamics.Transform);
            }
        }

        [Test]
        public void ApplyErrors_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearableModuleError.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.Apply(report);

            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.NoArmatureInWearable), "Should have NoArmatureInWearable error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.ApplyingModuleHasErrors), "Should have ApplyingModuleHasErrors error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.ApplyingWearableHasErrors), "Should have ApplyingWearableHasErrors error");
        }
    }
}
