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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Chocopoi.DressingFramework.Proxy;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Wearable.Modules
{
    public class CabinetAnimWearableModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new CabinetAnimWearableModuleProvider();
            var makeUpConfig = new CabinetAnimWearableModuleConfig();
            var deserializedConfig = (CabinetAnimWearableModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.version.ToString(), deserializedConfig.version.ToString());
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new CabinetAnimWearableModuleProvider();
            Assert.IsInstanceOf(typeof(CabinetAnimWearableModuleConfig), provider.NewModuleConfig());
        }

        private void InvokeProvider(GameObject avatarObj, Transform wearableTrans, bool preview, out List<IDynamicsProxy> wearableDynamics)
        {
            var provider = new CabinetAnimWearableModuleProvider();

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var bsm = wearCtx.wearableConfig.FindModuleConfig<CabinetAnimWearableModuleConfig>();
            Assert.NotNull(bsm);
            wearableDynamics = wearCtx.wearableDynamics;

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = CabinetAnimWearableModuleConfig.ModuleIdentifier,
                config = bsm
            }}), preview));
        }

        private static void GetTestObjects(GameObject avatarObj, Transform wearableTrans, out Transform aRoot1, out Transform aRoot2, out Transform aRoot3, out Transform abc, out SkinnedMeshRenderer abcSmr, out Transform wRoot1, out Transform wRoot2, out Transform wRoot3, out SkinnedMeshRenderer wbcSmr)
        {
            aRoot1 = avatarObj.transform.Find("ARoot1");
            Assert.NotNull(aRoot1);
            aRoot2 = avatarObj.transform.Find("ARoot2");
            Assert.NotNull(aRoot2);
            aRoot3 = avatarObj.transform.Find("ARoot3");
            Assert.NotNull(aRoot3);
            abc = avatarObj.transform.Find("AvatarBlendshapeCube");
            Assert.NotNull(abc);
            abcSmr = abc.GetComponent<SkinnedMeshRenderer>();
            Assert.NotNull(abcSmr);

            wRoot1 = wearableTrans.Find("WRoot1");
            Assert.NotNull(wRoot1);
            wRoot2 = wearableTrans.Find("WRoot2");
            Assert.NotNull(wRoot2);
            wRoot3 = wearableTrans.Find("WRoot3");
            Assert.NotNull(wRoot3);
            var wbc = wearableTrans.Find("WearableBlendshapeCube");
            Assert.NotNull(wbc);
            wbcSmr = wbc.GetComponent<SkinnedMeshRenderer>();
            Assert.NotNull(wbcSmr);
        }

        [Test]
        public void InvokePreviewTest()
        {
            var avatarObj = InstantiateEditorTestPrefab("DTTest_CabinetAnimAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            InvokeProvider(avatarObj, wearableTrans, true, out _);
            GetTestObjects(avatarObj, wearableTrans, out var aRoot1, out var aRoot2, out var aRoot3, out var abc, out var abcSmr, out var wRoot1, out var wRoot2, out var wRoot3, out var wbcSmr);

            Assert.False(aRoot1.gameObject.activeSelf);
            Assert.True(aRoot2.gameObject.activeSelf);
            Assert.False(aRoot3.gameObject.activeSelf);

            Assert.True(wRoot1.gameObject.activeSelf);
            Assert.True(wRoot2.gameObject.activeSelf);
            Assert.True(wRoot3.gameObject.activeSelf);

            Assert.True(Mathf.Approximately(40.0f, abcSmr.GetBlendShapeWeight(0)));
            Assert.True(Mathf.Approximately(60.0f, wbcSmr.GetBlendShapeWeight(0)));
        }

        [Test]
        public void InvokeNonPreviewTest()
        {
            var avatarObj = InstantiateEditorTestPrefab("DTTest_CabinetAnimAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            InvokeProvider(avatarObj, wearableTrans, false, out var wearableDynamics);
            GetTestObjects(avatarObj, wearableTrans, out var aRoot1, out var aRoot2, out var aRoot3, out _, out _, out var wRoot1, out var wRoot2, out var wRoot3, out _);

            Assert.True(aRoot1.gameObject.activeSelf);
            Assert.True(aRoot2.gameObject.activeSelf);
            Assert.True(aRoot3.gameObject.activeSelf);

            Assert.False(wRoot1.gameObject.activeSelf);
            Assert.False(wRoot2.gameObject.activeSelf);
            Assert.False(wRoot3.gameObject.activeSelf);

            foreach (var dynamics in wearableDynamics)
            {
                Assert.False(dynamics.GameObject.activeSelf, $"Dynamics expected to be inactive: {dynamics.GameObject.name}");
            }
        }
    }
}
