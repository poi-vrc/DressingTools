/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class BlendshapeSyncPassTest : EditorTestBase
    {
        [Test]
        public void InvokeTest()
        {
            var pass = new BlendshapeSyncPass();
            var avatar = CreateGameObject("Avatar");
            var ctx = new DKNativeContext(avatar);

            var originalAnim = LoadEditorTestAsset<AnimationClip>("DTTest_BlendshapeSyncAnim.anim");
            var animStore = ctx.Feature<AnimationStore>();
            animStore.RegisterClip(originalAnim, (AnimationClip clip) => { });
            Assert.AreEqual(1, animStore.Clips.Count);

            var avatarObj = InstantiateEditorTestPrefab("DTTest_BlendshapeSyncAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var wearableBlendshapeCube = wearableTrans.Find("WearableBlendshapeCube");
            Assert.NotNull(wearableBlendshapeCube);
            Assert.True(wearableBlendshapeCube.TryGetComponent<SkinnedMeshRenderer>(out var wearableSmr));

            var comp = avatar.AddComponent<DTBlendshapeSync>();
            comp.Entries.Add(new DTBlendshapeSync.Entry()
            {
                SourcePath = "AvatarBlendshapeCube",
                SourceBlendshape = "SomeKey",
                DestinationSkinnedMeshRenderer = wearableSmr,
                DestinationBlendshape = "SomeKey"
            });

            Assert.True(pass.Invoke(ctx));

            var originalCurveBindings = AnimationUtility.GetCurveBindings(originalAnim);
            Assert.AreEqual(1, originalCurveBindings.Length);
            var originalCurveBinding = originalCurveBindings[0];
            var originalCurve = AnimationUtility.GetEditorCurve(originalAnim, originalCurveBinding);

            var newClip = animStore.Clips[0].newClip;
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
