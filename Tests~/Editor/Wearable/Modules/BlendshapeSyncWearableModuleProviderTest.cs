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
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Wearable.Modules
{
    public class BlendshapeSyncWearableModuleProviderTest : EditorTestBase
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

            var originalAnim = LoadEditorTestAsset<AnimationClip>("DTTest_BlendshapeSyncAnim.anim");

            var avatarObj = InstantiateEditorTestPrefab("DTTest_BlendshapeSyncAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var cabCtx = CreateCabinetContext(avatarObj);
            cabCtx.animationStore.RegisterClip(originalAnim, (AnimationClip clip) => { });
            Assert.AreEqual(1, cabCtx.animationStore.Clips.Count);

            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var bsm = wearCtx.wearableConfig.FindModuleConfig<BlendshapeSyncWearableModuleConfig>();
            Assert.NotNull(bsm);

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = BlendshapeSyncWearableModuleConfig.ModuleIdentifier,
                config = bsm
            }}), false));

            var originalCurveBindings = AnimationUtility.GetCurveBindings(originalAnim);
            Assert.AreEqual(1, originalCurveBindings.Length);
            var originalCurveBinding = originalCurveBindings[0];
            var originalCurve = AnimationUtility.GetEditorCurve(originalAnim, originalCurveBinding);

            var newClip = cabCtx.animationStore.Clips[0].newClip;
            Assert.NotNull(newClip);
            var newClipCurveBindings = AnimationUtility.GetCurveBindings(newClip);
            Assert.AreEqual(2, newClipCurveBindings.Length);

            var expectedPaths = new string[] { "AvatarBlendshapeCube", "Wearable/WearableBlendshapeCube" };

            // assert curve bindings
            foreach (var curveBinding in newClipCurveBindings)
            {
                var found = false;
                foreach (var path in expectedPaths)
                {
                    if (path == curveBinding.path)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.True(found, "Curve bindings contain unexpected paths");
                Assert.AreEqual(originalCurve, AnimationUtility.GetEditorCurve(newClip, curveBinding), "Expected new clip curve bindings should be then same as original clip");
            }
        }
    }
}
