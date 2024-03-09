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
using Chocopoi.DressingTools.Animations;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

#if DT_VRCSDK3A
using VRC.SDK3.Avatars.Components;
#endif

namespace Chocopoi.DressingTools.Tests.Animations
{
    internal class AnimUtilsTest : EditorTestBase
    {
        [Test]
        public void ScanAnimatorParametersTest_NoVRC()
        {
            // just for passing coverage
            var avatar = CreateGameObject("Avatar");
            var result = AnimUtils.ScanAnimatorParameters(avatar);
            Assert.AreEqual(result.Count, 0);
        }

#if DT_VRCSDK3A
        [Test]
        public void ScanAnimatorParametersTest_VRC()
        {
            // just for passing coverage
            var avatar = CreateGameObject("Avatar");

            var avatarDesc = avatar.AddComponent<VRCAvatarDescriptor>();

            var ctrl1 = new AnimatorController();
            var layer1 = new VRCAvatarDescriptor.CustomAnimLayer
            {
                type = VRCAvatarDescriptor.AnimLayerType.Base,
                isDefault = false,
                isEnabled = true,
                animatorController = ctrl1
            };
            ctrl1.parameters = new AnimatorControllerParameter[] {
                new AnimatorControllerParameter() {
                    name = "Abc",
                    type = AnimatorControllerParameterType.Int
                },
                new AnimatorControllerParameter() {
                    name = "Def",
                    type = AnimatorControllerParameterType.Bool
                },
                new AnimatorControllerParameter() {
                    name = "TrackingType", // mock some VRC internal params to expect it to ignore
                    type = AnimatorControllerParameterType.Int
                }
            };

            var ctrl2 = new AnimatorController();
            var layer2 = new VRCAvatarDescriptor.CustomAnimLayer
            {
                type = VRCAvatarDescriptor.AnimLayerType.FX,
                isDefault = false,
                isEnabled = true,
                animatorController = ctrl2
            };
            ctrl2.parameters = new AnimatorControllerParameter[] {
                new AnimatorControllerParameter() {
                    name = "Abc",
                    type = AnimatorControllerParameterType.Bool // mock mixed types
                },
                new AnimatorControllerParameter() {
                    name = "Def",
                    type = AnimatorControllerParameterType.Bool
                },
            };

            avatarDesc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] { layer1, layer2 };
            avatarDesc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var result = AnimUtils.ScanAnimatorParameters(avatar);

            Assert.AreEqual(2, result.Count);
            Assert.True(result.Keys.Contains("Abc"));
            Assert.True(result.Keys.Contains("Def"));
            Assert.False(result.Keys.Contains("TrackingMode"));
            Assert.AreEqual(AnimatorControllerParameterType.Int, result["Abc"]);
            Assert.AreEqual(AnimatorControllerParameterType.Bool, result["Def"]);
        }
#endif

        private static void PrepareAnimatorController(out AnimatorController ctrl, out AnimatorState state11, out AnimatorState state12, out AnimatorState state21, out AnimatorState state22)
        {
            ctrl = new AnimatorController();
            ctrl.AddLayer("1");
            ctrl.AddLayer("2");
            var layer1 = ctrl.layers[0];
            var layer2 = ctrl.layers[1];

            state11 = layer1.stateMachine.AddState("11");
            state12 = layer1.stateMachine.AddState("12");
            state21 = layer2.stateMachine.AddState("21");
            state22 = layer2.stateMachine.AddState("22");
        }

        [Test]
        public void GetWriteDefaultCountsTest()
        {
            PrepareAnimatorController(out var ctrl, out var state11, out var state12, out var state21, out var state22);

            state11.writeDefaultValues = true;
            state12.writeDefaultValues = true;
            state21.writeDefaultValues = true;
            state22.writeDefaultValues = true;
            AnimUtils.GetWriteDefaultCounts(ctrl, out var onCount, out var offCount);
            Assert.AreEqual(4, onCount);
            Assert.AreEqual(0, offCount);

            state11.writeDefaultValues = false;
            state12.writeDefaultValues = false;
            state21.writeDefaultValues = false;
            state22.writeDefaultValues = false;
            AnimUtils.GetWriteDefaultCounts(ctrl, out onCount, out offCount);
            Assert.AreEqual(0, onCount);
            Assert.AreEqual(4, offCount);

            state11.writeDefaultValues = false;
            state12.writeDefaultValues = false;
            state21.writeDefaultValues = true;
            state22.writeDefaultValues = false;
            AnimUtils.GetWriteDefaultCounts(ctrl, out onCount, out offCount);
            Assert.AreEqual(1, onCount);
            Assert.AreEqual(3, offCount);
        }

        [Test]
        public void DetermineWriteDefaultsByOnOffCountsTest()
        {
            Assert.True(AnimUtils.DetermineWriteDefaultsByOnOffCounts(3, 0));
            Assert.False(AnimUtils.DetermineWriteDefaultsByOnOffCounts(0, 3));
            Assert.True(AnimUtils.DetermineWriteDefaultsByOnOffCounts(3, 3));
        }

        [Test]
        public void DetectWriteDefaultsTest()
        {
            PrepareAnimatorController(out var ctrl, out var state11, out var state12, out var state21, out var state22);

            state11.writeDefaultValues = true;
            state12.writeDefaultValues = true;
            state21.writeDefaultValues = true;
            state22.writeDefaultValues = true;
            Assert.True(AnimUtils.DetectWriteDefaults(ctrl));

            state11.writeDefaultValues = false;
            state12.writeDefaultValues = false;
            state21.writeDefaultValues = false;
            state22.writeDefaultValues = false;
            Assert.False(AnimUtils.DetectWriteDefaults(ctrl));

            state11.writeDefaultValues = true;
            state12.writeDefaultValues = true;
            state21.writeDefaultValues = false;
            state22.writeDefaultValues = false;
            Assert.True(AnimUtils.DetectWriteDefaults(ctrl));

            state11.writeDefaultValues = true;
            state12.writeDefaultValues = false;
            state21.writeDefaultValues = false;
            state22.writeDefaultValues = false;
            Assert.False(AnimUtils.DetectWriteDefaults(ctrl));
        }
    }
}
