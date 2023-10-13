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
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Api.Integration.VRChat.Wearable.Modules;
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Integrations.VRChat
{
    public class VRCMergeAnimLayerWearableModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new VRCMergeAnimLayerWearableModuleProvider();
            var makeUpConfig = new VRCMergeAnimLayerWearableModuleConfig();
            var deserializedConfig = (VRCMergeAnimLayerWearableModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.animLayer, deserializedConfig.animLayer);
            Assert.AreEqual(makeUpConfig.matchLayerWriteDefaults, deserializedConfig.matchLayerWriteDefaults);
            Assert.AreEqual(makeUpConfig.animatorPath, deserializedConfig.animatorPath);
            Assert.AreEqual(makeUpConfig.removeAnimatorAfterApply, deserializedConfig.removeAnimatorAfterApply);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new VRCMergeAnimLayerWearableModuleProvider();
            Assert.IsInstanceOf(typeof(VRCMergeAnimLayerWearableModuleConfig), provider.NewModuleConfig());
        }

        [Test]
        public void InvokeTest()
        {
            var provider = new VRCMergeAnimLayerWearableModuleProvider();
            var avatarObj = InstantiateEditorTestPrefab("VRCMergeAnimatorsAvatar.prefab");

            var wearable1Trans = avatarObj.transform.Find("Wearable1");
            Assert.NotNull(wearable1Trans);
            var wearable2Trans = avatarObj.transform.Find("Wearable2");
            Assert.NotNull(wearable2Trans);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wear1Ctx = CreateWearableContext(cabCtx, wearable1Trans.gameObject);
            var wear2Ctx = CreateWearableContext(cabCtx, wearable2Trans.gameObject);

            var malm1 = wear1Ctx.wearableConfig.FindModuleConfig<VRCMergeAnimLayerWearableModuleConfig>();
            var malm2 = wear1Ctx.wearableConfig.FindModuleConfig<VRCMergeAnimLayerWearableModuleConfig>();

            Assert.True(provider.Invoke(
                cabCtx,
                wear1Ctx,
                new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                    moduleName = VRCMergeAnimLayerWearableModuleConfig.ModuleIdentifier,
                    config = malm1
                } }),
                false), "Provider invoke on wearable 1 returned failure");

            Assert.True(provider.Invoke(
                cabCtx,
                wear2Ctx,
                new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                    moduleName = VRCMergeAnimLayerWearableModuleConfig.ModuleIdentifier,
                    config = malm2
                } }),
                false), "Provider invoke on wearable 2 returned failure");

            // TODO: check content
        }
    }
}
