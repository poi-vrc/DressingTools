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

#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingTools.Integrations.VRChat;
using NUnit.Framework;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Tests.OneConf.Integrations.VRChat
{
    public class VRCEditorUtilsTest : EditorTestBase
    {
        [Test]
        public void CopyAndReplaceExpressionParametersTest()
        {
            var avatarObj = CreateGameObject("Avatar");
            var avatarDesc = avatarObj.AddComponent<VRCAvatarDescriptor>();
            var path = $"{ApplyCabinetContext.GeneratedAssetsPath}/VRCEditorUtilsTest_ExParams.asset";
            var exParams = VRCEditorUtils.CopyAndReplaceExpressionParameters(avatarDesc, path);
            // we just check not null here, we should actually compare the content actually
            Assert.NotNull(exParams);

            // do not copy again if already copied before
            var cachedExParams = VRCEditorUtils.CopyAndReplaceExpressionParameters(avatarDesc, path);
            Assert.NotNull(cachedExParams);
            Assert.AreEqual(exParams, cachedExParams);

            Assert.AreEqual(exParams, avatarDesc.expressionParameters);
        }

        [Test]
        public void CopyAndReplaceExpressionMenuTest()
        {
            var avatarObj = CreateGameObject("Avatar");
            var avatarDesc = avatarObj.AddComponent<VRCAvatarDescriptor>();
            var path = $"{ApplyCabinetContext.GeneratedAssetsPath}/VRCEditorUtilsTest_ExMenu.asset";
            var exMenu = VRCEditorUtils.CopyAndReplaceExpressionMenu(avatarDesc, path);
            // we just check not null here, we should actually compare the content actually
            Assert.NotNull(exMenu);

            // do not copy again if already copied before
            var cachedExMenu = VRCEditorUtils.CopyAndReplaceExpressionMenu(avatarDesc, path);
            Assert.NotNull(cachedExMenu);
            Assert.AreEqual(exMenu, cachedExMenu);

            Assert.AreEqual(exMenu, avatarDesc.expressionsMenu);
        }

        private static void AssertAnimLayer(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType, AnimatorController animator)
        {
            VRCEditorUtils.FindAnimLayerArrayAndIndex(avatarDescriptor, animLayerType, out var layers, out var customAnimLayerIndex);
            Assert.NotNull(layers, $"{animLayerType} layer not found");
            Assert.AreNotEqual(-1, customAnimLayerIndex, $"{animLayerType} layer not found");

            Assert.AreEqual(animator, layers[customAnimLayerIndex].animatorController, $"{animLayerType} animator not replaced in avatar");
        }

        [Test]
        public void CopyAndReplaceLayerAnimatorTest()
        {
            var avatarObj = CreateGameObject("Avatar");
            var avatarDesc = avatarObj.AddComponent<VRCAvatarDescriptor>();
            avatarDesc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] {
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.Base },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.Additive },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.Gesture },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.Action },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.FX }
            };
            avatarDesc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] {
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.Sitting },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.TPose },
                new VRCAvatarDescriptor.CustomAnimLayer() { type = VRCAvatarDescriptor.AnimLayerType.IKPose }
            };

            var layers = new VRCAvatarDescriptor.AnimLayerType[]{
                VRCAvatarDescriptor.AnimLayerType.Base,
                VRCAvatarDescriptor.AnimLayerType.Additive,
                VRCAvatarDescriptor.AnimLayerType.Gesture,
                VRCAvatarDescriptor.AnimLayerType.Action,
                VRCAvatarDescriptor.AnimLayerType.FX,
                VRCAvatarDescriptor.AnimLayerType.Sitting,
                VRCAvatarDescriptor.AnimLayerType.TPose,
                VRCAvatarDescriptor.AnimLayerType.IKPose
            };

            foreach (var layer in layers)
            {
                var path = $"{ApplyCabinetContext.GeneratedAssetsPath}/VRCEditorUtilsTest_AnimLayer_{layer}.asset";
                var animator = VRCEditorUtils.CopyAndReplaceLayerAnimator(avatarDesc, layer, path);
                // we just check not null here, we should actually compare the content actually
                Assert.NotNull(animator, $"{layer} animator copy null");

                // do not copy again if already copied before
                var cachedAnimator = VRCEditorUtils.CopyAndReplaceLayerAnimator(avatarDesc, layer, path);
                Assert.NotNull(cachedAnimator, $"{layer} animator copy cache null");
                Assert.AreEqual(animator, cachedAnimator, $"{layer} animator copy not same as cache");

                AssertAnimLayer(avatarDesc, layer, animator);
            }
        }
    }
}
#endif
