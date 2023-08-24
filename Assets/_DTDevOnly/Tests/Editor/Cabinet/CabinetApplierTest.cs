using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class CabinetApplierTest : DTEditorTestBase
    {
        private static void ApplyCabinet(DTReport report, DTCabinet cabinet)
        {
            new CabinetApplier(report, cabinet).Execute();
        }

        [Test]
        public void AvatarWithOneWearable_AppliesNormally()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            ApplyCabinet(report, cabinet);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");
        }

        // TODO: new test for version

        // [Test]
        // public void ConfigVersionTooNew_ReturnsCorrectErrorCodes()
        // {
        //     var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
        //     var cabinet = avatarRoot.GetComponent<DTCabinet>();

        //     // we simulate this by adding the config version by one
        //     var wearableComp = avatarRoot.GetComponentInChildren<DTCabinetWearable>();
        //     Assert.NotNull(wearableComp);
        //     JObject json = JObject.Parse(wearableComp.configJson);
        //     json["configVersion"] = WearableConfig.CurrentConfigVersion + 1;
        //     wearableComp.configJson = json.ToString(Formatting.None);

        //     var report = new DTReport();
        //     ApplyCabinet(report, cabinet);

        //     Assert.True(report.HasLogCode(CabinetApplier.MessageCode.IncompatibleConfigVersion), "Should have incompatible config version error");
        // }

        // TODO: write config migration test

        [Test]
        public void ConfigDeserializationFailure_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            // we simulate this by destroying the config json
            var wearableComp = avatarRoot.GetComponentInChildren<DTCabinetWearable>();
            Assert.NotNull(wearableComp);
            wearableComp.configJson = "ababababababababa";

            var report = new DTReport();
            ApplyCabinet(report, cabinet);

            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.UnableToDeserializeConfig), "Should have deserialization error");
        }

        [Test]
        public void GroupDynamicsToSeparateGameObjectsCorrectly()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableOtherDynamics.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.groupDynamics = true;
            cabinet.groupDynamicsSeparateGameObjects = true;
            ApplyCabinet(report, cabinet);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");

            // get wearable root
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);

            // get dynamics container
            var dynamicsContainer = wearableRoot.Find("DT_Dynamics");
            Assert.NotNull(dynamicsContainer);

            // check dynamics
            var wearableDynamicsList = DTEditorUtils.ScanDynamics(wearableRoot.gameObject);
            foreach (var wearableDynamics in wearableDynamicsList)
            {
                Assert.AreEqual(dynamicsContainer, wearableDynamics.Transform.parent);
            }
        }

        [Test]
        public void GroupDynamicsToSingleGameObjectCorrectly()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableOtherDynamics.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            cabinet.groupDynamics = true;
            cabinet.groupDynamicsSeparateGameObjects = false;
            ApplyCabinet(report, cabinet);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");

            // get wearable root
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);

            // get dynamics container
            var dynamicsContainer = wearableRoot.Find("DT_Dynamics");
            Assert.NotNull(dynamicsContainer);

            // check dynamics
            var wearableDynamicsList = DTEditorUtils.ScanDynamics(wearableRoot.gameObject);
            foreach (var wearableDynamics in wearableDynamicsList)
            {
                Assert.AreEqual(dynamicsContainer, wearableDynamics.Transform);
            }
        }

        [Test]
        public void ApplyErrors_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableModuleError.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();

            var report = new DTReport();
            ApplyCabinet(report, cabinet);

            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.NoArmatureInWearable), "Should have NoArmatureInWearable error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.ApplyingModuleHasErrors), "Should have ApplyingModuleHasErrors error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.ApplyingWearableHasErrors), "Should have ApplyingWearableHasErrors error");
        }
    }
}
