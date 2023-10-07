using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Dresser;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class CabinetApplierTest : DTEditorTestBase
    {
        private static void ApplyCabinet(DKReport report, DTCabinet cabinet)
        {
            new CabinetApplier(report, cabinet).RunStages();
        }

        [Test]
        public void ApplyErrors_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableModuleError.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var report = new DKReport();
            ApplyCabinet(report, cabinet);

            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.NoArmatureInWearable), "Should have NoArmatureInWearable error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.WearableHookHasErrors), "Should have WearableHookHasErrors error");
        }
    }
}
