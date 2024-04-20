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

using System.Linq;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Animations.VRChat;
using VRC.SDK3.Avatars.Components;
#endif

namespace Chocopoi.DressingTools.Tests.Passes.Animations
{
    // current this test is VRC only
    internal class ManipulateAnimatorPassTest : EditorTestBase
    {
#if DT_VRCSDK3A
        private static void AssertAddResult(AnimatorController ctrl)
        {
            Assert.AreEqual(3, ctrl.parameters.Length);
            Assert.AreEqual(1, ctrl.parameters
                .Where(p =>
                    p.name == "SomeFloat" &&
                    p.type == AnimatorControllerParameterType.Float &&
                    p.defaultFloat == 0.75)
                .Count());
            Assert.AreEqual(1, ctrl.parameters
                .Where(p =>
                    p.name == "SomeBool" &&
                    p.type == AnimatorControllerParameterType.Bool &&
                    p.defaultBool)
                .Count());
            Assert.AreEqual(1, ctrl.parameters
                .Where(p =>
                    p.name == "SomeInt" &&
                    p.type == AnimatorControllerParameterType.Int &&
                    p.defaultInt == 127)
                .Count());

            for (var i = 0; i < ctrl.layers.Length; i++)
            {
                Debug.Log($"{i}: {ctrl.layers[i].name}");
            }
            Assert.AreEqual(4, ctrl.layers.Length);
            {
                var baseLayer = ctrl.layers[0];
                Assert.NotNull(baseLayer);
                Assert.AreEqual(2, baseLayer.stateMachine.states.Length);

                var state1 = baseLayer.stateMachine.states.Where(s => s.state.name == "1").FirstOrDefault();
                Assert.NotNull(state1);
                Assert.AreEqual(1, state1.state.transitions
                    .Where(t => t.conditions
                        .Where(c =>
                            c.parameter == "SomeBool" &&
                            c.mode == AnimatorConditionMode.If)
                        .Count() == 1)
                    .Count());

                var state2 = baseLayer.stateMachine.states.Where(s => s.state.name == "2").FirstOrDefault();
                Assert.NotNull(state2);
                Assert.AreEqual(1, state2.state.transitions
                    .Where(t => t.conditions
                        .Where(c =>
                            c.parameter == "SomeBool" &&
                            c.mode == AnimatorConditionMode.IfNot)
                        .Count() == 1)
                    .Count());
            }
            {
                var layer1 = ctrl.layers[1];
                Assert.NotNull(layer1);
                Assert.AreEqual(1, layer1.stateMachine.states.Length);
                Assert.AreEqual(1, layer1.stateMachine.states.Where(s => s.state.name == "3").Count());
            }
            {
                var layer2 = ctrl.layers[2];
                Assert.NotNull(layer2);
                Assert.AreEqual(1, layer2.stateMachine.states.Length);
                Assert.AreEqual(1, layer2.stateMachine.states.Where(s => s.state.name == "4").Count());
            }
            {
                var layer3 = ctrl.layers[3];
                Assert.NotNull(layer3);
                Assert.AreEqual(1, layer3.stateMachine.states.Length);
                Assert.AreEqual(1, layer3.stateMachine.states.Where(s => s.state.name == "5").Count());
            }
        }

        [Test]
        public void AddTest()
        {
            var avatar = CreateGameObject("Avatar");
            var avatarDesc = avatar.AddComponent<VRCAvatarDescriptor>();
            var gestureCtrl = Object.Instantiate(LoadEditorTestAsset<AnimatorController>("DummySource.controller"));
            var fxCtrl = Object.Instantiate(LoadEditorTestAsset<AnimatorController>("DummySource.controller"));
            avatarDesc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] {
                new VRCAvatarDescriptor.CustomAnimLayer() {
                    isEnabled = true,
                    isDefault = false,
                    type = VRCAvatarDescriptor.AnimLayerType.Gesture,
                    animatorController = gestureCtrl
                },
                new VRCAvatarDescriptor.CustomAnimLayer() {
                    isEnabled = true,
                    isDefault = false,
                    type = VRCAvatarDescriptor.AnimLayerType.FX,
                    animatorController = fxCtrl
                }
            };
            avatarDesc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var ctrlToAdd = LoadEditorTestAsset<AnimatorController>("DummyAdd.controller");

            var comp1Obj = CreateGameObject("1", avatar.transform);
            var comp1 = comp1Obj.AddComponent<DTManipulateAnimator>();
            comp1.ManipulateMode = DTManipulateAnimator.ManipulateModes.Add;
            comp1.TargetType = DTManipulateAnimator.TargetTypes.VRCAnimLayer;
            comp1.VRCTargetLayer = VRCAvatarDescriptor.AnimLayerType.Gesture;
            comp1.SourceType = DTManipulateAnimator.SourceTypes.AnimatorController;
            comp1.SourceRelativeRoot = null;
            comp1.SourceController = ctrlToAdd;

            var comp2Obj = CreateGameObject("2", avatar.transform);
            var comp2 = comp2Obj.AddComponent<DTManipulateAnimator>();
            comp2.ManipulateMode = DTManipulateAnimator.ManipulateModes.Add;
            comp2.TargetType = DTManipulateAnimator.TargetTypes.VRCAnimLayer;
            comp2.VRCTargetLayer = VRCAvatarDescriptor.AnimLayerType.FX;
            comp2.SourceType = DTManipulateAnimator.SourceTypes.Animator;
            comp2.SourceAnimator = null;
            var animator = comp2Obj.AddComponent<Animator>();
            animator.runtimeAnimatorController = ctrlToAdd;

            var pass = new ManipulateAnimatorPass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            // the controllers got replaced, we need to find them again
            var mergedGestureCtrl = VRCAnimUtils.GetAvatarLayerAnimator(avatarDesc, VRCAvatarDescriptor.AnimLayerType.Gesture);
            var mergedFxCtrl = VRCAnimUtils.GetAvatarLayerAnimator(avatarDesc, VRCAvatarDescriptor.AnimLayerType.FX);
            AssertAddResult(mergedGestureCtrl);
            AssertAddResult(mergedFxCtrl);
        }
#endif
    }
}
