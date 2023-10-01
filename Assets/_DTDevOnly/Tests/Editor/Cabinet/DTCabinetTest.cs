using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Logging;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class DTCabinetTest : DTEditorTestBase
    {
        private static void ApplyCabinet(DKReport report, DTCabinet cabinet)
        {
            new CabinetApplier(report, cabinet).RunStages();
        }

        [Test]
        public void GetterSetterTest()
        {
            var cabinetGo = CreateGameObject("GetterSetterTestGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();

            var obj = CreateGameObject("SomeGameObject");
            cabinet.AvatarGameObject = obj;
            Assert.AreEqual(obj, cabinet.AvatarGameObject);
        }

        [Test]
        public void GetWearables_NoAvatarGameObject_ReturnsEmptyArray()
        {
            var cabinetGo = CreateGameObject("GetWearablesGameObject");
            var wearables = DKRuntimeUtils.GetCabinetWearables(cabinetGo);
            Assert.NotNull(wearables);
            Assert.AreEqual(0, wearables.Length);
        }

        [Test]
        public void GetWearables_ReturnsOneWearable()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);
            Assert.NotNull(cabinet.AvatarGameObject);

            var wearables = DKRuntimeUtils.GetCabinetWearables(cabinet.AvatarGameObject);
            Assert.NotNull(wearables);
            Assert.AreEqual(1, wearables.Length);

            Assert.NotNull(wearables[0]);
            Assert.AreEqual("DTTest_PhysBoneWearable", wearables[0].WearableGameObject.name);
        }

        [Test]
        public void ApplyInEditorMode_AppliesNormally()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var report = new DKReport();
            ApplyCabinet(report, cabinet);

            Assert.False(report.HasLogType(LogType.Error), "Should have no errors");
        }
    }
}
