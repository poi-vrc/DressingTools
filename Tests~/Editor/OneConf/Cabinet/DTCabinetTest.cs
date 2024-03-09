/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.OneConf.Cabinet
{
    internal class DTCabinetTest : EditorTestBase
    {
        [Test]
        public void GetterSetterTest()
        {
            var cabinetGo = CreateGameObject("GetterSetterTestGameObject");
            var cabinet = cabinetGo.AddComponent<DTCabinet>();

            var obj = CreateGameObject("SomeGameObject");
            cabinet.RootGameObject = obj;
            Assert.AreEqual(obj, cabinet.RootGameObject);
        }

        [Test]
        public void GetWearables_NoAvatarGameObject_ReturnsEmptyArray()
        {
            var cabinetGo = CreateGameObject("GetWearablesGameObject");
            var wearables = OneConfUtils.GetCabinetWearables(cabinetGo);
            Assert.NotNull(wearables);
            Assert.AreEqual(0, wearables.Length);
        }

        [Test]
        public void GetWearables_ReturnsOneWearable()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);
            Assert.NotNull(cabinet.RootGameObject);

            var wearables = OneConfUtils.GetCabinetWearables(cabinet.RootGameObject);
            Assert.NotNull(wearables);
            Assert.AreEqual(1, wearables.Length);

            Assert.NotNull(wearables[0]);
            Assert.AreEqual("DTTest_PhysBoneWearable", wearables[0].RootGameObject.name);
        }

        [Test]
        public void ApplyInEditorMode_AppliesNormally()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");

            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var ab = new AvatarBuilder(avatarRoot);
            ab.RunStages();
            var report = (DKReport)ab.Context.Report;

            Assert.False(report.HasLogType(LogType.Error), "Should have no errors");
        }
    }
}
