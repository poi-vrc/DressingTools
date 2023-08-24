using System.Collections;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class DTCabinetTest : DTEditorTestBase
    {
        private static void ApplyCabinet(DTReport report, DTCabinet cabinet)
        {
            new CabinetApplier(report, cabinet).Execute();
        }

        [Test]
        public void GetterSetterTest()
        {
            var cabinetGo = CreateGameObject("GetterSetterTestGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();

            var obj = CreateGameObject("SomeGameObject");
            cabinet.avatarGameObject = obj;
            Assert.AreEqual(obj, cabinet.avatarGameObject);

            var randomString = "SomeString";
            cabinet.avatarArmatureName = randomString;
            Assert.AreEqual(randomString, cabinet.avatarArmatureName);

            var val = !cabinet.groupDynamics;
            cabinet.groupDynamics = val;
            Assert.AreEqual(val, cabinet.groupDynamics);

            val = !cabinet.groupDynamicsSeparateGameObjects;
            cabinet.groupDynamicsSeparateGameObjects = val;
            Assert.AreEqual(val, cabinet.groupDynamicsSeparateGameObjects);
        }

        [Test]
        public void GetWearables_NoAvatarGameObject_ReturnsEmptyArray()
        {
            var cabinetGo = CreateGameObject("GetWearablesGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();
            cabinet.avatarGameObject = null;

            Assert.IsNull(cabinet.avatarGameObject);

            var wearables = DTEditorUtils.GetCabinetWearables(cabinet);
            Assert.NotNull(wearables);
            Assert.AreEqual(0, wearables.Length);
        }

        [Test]
        public void GetWearables_ReturnsOneWearable()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var wearables = DTEditorUtils.GetCabinetWearables(cabinet);
            Assert.NotNull(wearables);
            Assert.AreEqual(1, wearables.Length);

            Assert.NotNull(wearables[0]);
            Assert.AreEqual("DTTest_PhysBoneWearable", wearables[0].transform.name);
        }

        [Test]
        public void ApplyInEditorMode_AppliesNormally()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var report = new DTReport();
            ApplyCabinet(report, cabinet);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");
        }
    }
}
