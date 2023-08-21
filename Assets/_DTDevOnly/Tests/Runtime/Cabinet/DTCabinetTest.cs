using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class DTCabinetTest : DTTestBase
    {
        [Test]
        public void GetterSetterTest()
        {
            var cabinetGo = CreateGameObject("GetterSetterTestGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();

            var obj = CreateGameObject("SomeGameObject");
            cabinet.AvatarGameObject = obj;
            Assert.AreEqual(obj, cabinet.AvatarGameObject);

            var randomString = "SomeString";
            cabinet.AvatarArmatureName = randomString;
            Assert.AreEqual(randomString, cabinet.AvatarArmatureName);

            var val = !cabinet.GroupDynamics;
            cabinet.GroupDynamics = val;
            Assert.AreEqual(val, cabinet.GroupDynamics);

            val = !cabinet.GroupDynamicsSeparateGameObjects;
            cabinet.GroupDynamicsSeparateGameObjects = val;
            Assert.AreEqual(val, cabinet.GroupDynamicsSeparateGameObjects);
        }

        [Test]
        public void GetWearables_NoAvatarGameObject_ReturnsEmptyArray()
        {
            var cabinetGo = CreateGameObject("GetWearablesGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();
            cabinet.AvatarGameObject = null;

            Assert.IsNull(cabinet.AvatarGameObject);

            var wearables = cabinet.GetWearables();
            Assert.NotNull(wearables);
            Assert.AreEqual(0, wearables.Length);
        }

        [Test]
        public void GetWearables_ReturnsOneWearable()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var wearables = cabinet.GetWearables();
            Assert.NotNull(wearables);
            Assert.AreEqual(1, wearables.Length);

            Assert.NotNull(wearables[0]);
            Assert.AreEqual("DTTest_PhysBoneWearable", wearables[0].transform.name);
        }

        [Test]
        public void ApplyInEditorMode_AppliesNormally()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var report = new DTReport();
            cabinet.Apply(report);

            Assert.False(report.HasLogType(DTReportLogType.Error), "Should have no errors");
        }

        [UnityTest]
        public IEnumerator ApplyInPlayModeOnLoad_AppliesNormally()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            yield return null;
            // we are unable to check DTReport logs so we just check is the armature empty here
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);
            var armature = wearableRoot.transform.Find("Armature");
            Assert.NotNull(armature);
            Assert.AreEqual(0, armature.childCount);
        }
    }
}
