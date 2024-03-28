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
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.OneConf.Wearable.Modules
{
    internal class BlendshapeSyncWearableModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new BlendshapeSyncWearableModuleProvider();
            var makeUpConfig = new BlendshapeSyncWearableModuleConfig();
            var deserializedConfig = (BlendshapeSyncWearableModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.version.ToString(), deserializedConfig.version.ToString());
            Assert.AreEqual(makeUpConfig.blendshapeSyncs.Count, deserializedConfig.blendshapeSyncs.Count);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new BlendshapeSyncWearableModuleProvider();
            Assert.IsInstanceOf(typeof(BlendshapeSyncWearableModuleConfig), provider.NewModuleConfig());
        }

        [Test]
        public void InvokeTest()
        {
            var provider = new BlendshapeSyncWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_BlendshapeSyncAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);
            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var bsm = wearCtx.wearableConfig.FindModuleConfig<BlendshapeSyncWearableModuleConfig>();
            Assert.NotNull(bsm);

            var wearableBlendshapeCube = wearableTrans.Find("WearableBlendshapeCube");
            Assert.NotNull(wearableBlendshapeCube);
            Assert.True(wearableBlendshapeCube.TryGetComponent<SkinnedMeshRenderer>(out var wearableSmr));

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = BlendshapeSyncWearableModuleConfig.ModuleIdentifier,
                config = bsm
            }}), false));

            Assert.True(wearableTrans.TryGetComponent<DTBlendshapeSync>(out var comp));
            Assert.AreEqual(1, comp.Entries.Count);
            Assert.AreEqual("AvatarBlendshapeCube", comp.Entries[0].SourcePath);
            Assert.AreEqual("SomeKey", comp.Entries[0].SourceBlendshape);
            Assert.AreEqual(wearableSmr, comp.Entries[0].DestinationSkinnedMeshRenderer);
            Assert.AreEqual("SomeKey", comp.Entries[0].DestinationBlendshape);
        }
    }
}
