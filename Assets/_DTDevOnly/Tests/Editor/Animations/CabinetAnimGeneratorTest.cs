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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Animation
{
    public class CabinetAnimGeneratorTest : EditorTestBase
    {
        private void CreateAnimGen(CabinetAnimWearableModuleConfig module, bool writeDefaults, out GameObject avatarObject, out GameObject wearableObject, out DKReport report, out CabinetAnimGenerator ag)
        {
            report = new DKReport();
            avatarObject = InstantiateEditorTestPrefab("DTTest_CabinetAnimGenAvatar.prefab");
            var wearableTransform = avatarObject.transform.Find("Wearable");
            Assert.NotNull(wearableTransform);
            wearableObject = wearableTransform.gameObject;

            var avatarDynamics = DKEditorUtils.ScanDynamics(avatarObject, true);
            var wearableDynamics = DKEditorUtils.ScanDynamics(wearableObject, false);
            var pathRemapper = new PathRemapper(avatarObject);
            ag = new CabinetAnimGenerator(report, avatarObject, module, wearableObject, avatarDynamics, wearableDynamics, pathRemapper, writeDefaults);
        }

        private static void AddToggle(List<CabinetAnimWearableModuleConfig.Toggle> toggles, string path, bool state)
        {
            toggles.Add(new CabinetAnimWearableModuleConfig.Toggle()
            {
                path = path,
                state = state
            });
        }

        private static void AddBlendshape(List<CabinetAnimWearableModuleConfig.BlendshapeValue> blendshapes, string path, float value)
        {
            blendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue()
            {
                path = path,
                blendshapeName = "SomeKey",
                value = value
            });
        }

        private static CabinetAnimWearableModuleConfig CreateModule()
        {
            var module = new CabinetAnimWearableModuleConfig();

            AddToggle(module.avatarAnimationOnWear.toggles, "SomeRootObject1", false);
            AddToggle(module.avatarAnimationOnWear.toggles, "SomeRootObject2", true);
            // SomeRootObject3

            AddToggle(module.wearableAnimationOnWear.toggles, "SomeWearableRootObject1", true);
            AddToggle(module.wearableAnimationOnWear.toggles, "SomeWearableRootObject2", false);
            // SomeWearableRootObject3

            AddBlendshape(module.avatarAnimationOnWear.blendshapes, "AvatarBlendshapeCube", 90.0f);
            AddBlendshape(module.wearableAnimationOnWear.blendshapes, "WearableBlendshapeCube", 80.0f);

            var toggleCustomizable = new CabinetAnimWearableModuleConfig.Customizable
            {
                name = "ToggleCustomizable",
                type = CabinetAnimWearableModuleConfig.CustomizableType.Toggle
            };
            AddToggle(toggleCustomizable.avatarToggles, "SomeRootObject1", true);
            AddToggle(toggleCustomizable.avatarToggles, "SomeRootObject2", false);
            AddToggle(toggleCustomizable.wearableToggles, "SomeWearableRootObject1", false);
            AddToggle(toggleCustomizable.wearableToggles, "SomeWearableRootObject2", true);
            AddBlendshape(toggleCustomizable.avatarBlendshapes, "AvatarBlendshapeCube", 60.0f);
            AddBlendshape(toggleCustomizable.wearableBlendshapes, "WearableBlendshapeCube", 70.0f);
            module.wearableCustomizables.Add(toggleCustomizable);

            var blendshapeCustomizable = new CabinetAnimWearableModuleConfig.Customizable
            {
                name = "BlendshapeCustomizable",
                type = CabinetAnimWearableModuleConfig.CustomizableType.Blendshape
            };
            AddBlendshape(blendshapeCustomizable.wearableBlendshapes, "WearableBlendshapeCube", 0.0f);
            module.wearableCustomizables.Add(blendshapeCustomizable);

            return module;
        }

        [Test]
        public void GenerateWearAnimations_NotGrandParent_ThrowsException()
        {
            var module = new CabinetAnimWearableModuleConfig();
            CreateAnimGen(module, true, out var avatarObject, out var wearableObject, out var report, out var ag);
            wearableObject.transform.SetParent(null);
            Assert.Throws(typeof(System.Exception), () => ag.GenerateWearAnimations());
        }

        private static void AssertCurve(AnimationClip clip, EditorCurveBinding[] bindings, System.Type type, string path, string propertyName, float value)
        {
            var found = false;

            foreach (var binding in bindings)
            {
                if (binding.type == type && binding.path == path && binding.propertyName == propertyName)
                {
                    found = true;
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    for (var i = 0; i < curve.keys.Length; i++)
                    {
                        var key = curve.keys[i];
                        Assert.AreEqual(value, key.value, "Curve values are not the same as expected");
                    }
                }
            }

            if (!found) Assert.Fail("Curve not found");
        }

        private void GenerateWearAnimations_NoRemapping_AllTests(bool writeDefaults)
        {
            var module = CreateModule();
            CreateAnimGen(module, writeDefaults, out var avatarObject, out var wearableObject, out var report, out var ag);

            var tuple = ag.GenerateWearAnimations();
            var enableClip = tuple.Item1;
            var disableClip = tuple.Item2;

            //
            // Enable clip
            //

            Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(enableClip).Length);
            var curveBindings = AnimationUtility.GetCurveBindings(enableClip);

            // avatar toggles
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeRootObject1", "m_IsActive", 0.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 1.0f);

            // wearable toggles
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject1", "m_IsActive", 1.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 0.0f);

            // blendshapes
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "AvatarBlendshapeCube", "blendShape.SomeKey", 90.0f);
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/WearableBlendshapeCube", "blendShape.SomeKey", 80.0f);

            // there is a dynamics at Wearable/Armature/Hips/MyDynBone
            // usually this will be grouped by cabinet applier to the root
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/Armature/Hips/MyDynBone", "m_IsActive", 1.0f);

            //
            // Disable clip
            //

            if (writeDefaults)
            {
                // for write defaults, disable clip should be all empty
                Assert.AreEqual(0, AnimationUtility.GetCurveBindings(disableClip).Length);
                Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(disableClip).Length);
            }
            else
            {
                // should be original values

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "AvatarBlendshapeCube", "blendShape.SomeKey", 20.0f);
                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/WearableBlendshapeCube", "blendShape.SomeKey", 30.0f);

                // turn off dynbone
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/Armature/Hips/MyDynBone", "m_IsActive", 0.0f);
            }
        }

        private void GenerateWearAnimations_WithRemapping_AllTests(bool writeDefaults)
        {
            var module = CreateModule();
            CreateAnimGen(module, writeDefaults, out var avatarObject, out var wearableObject, out var report, out var ag);

            // move the SomeRootObject1 away somewhere to trigger remapping
            var ro1 = avatarObject.transform.Find("SomeRootObject1");
            Assert.NotNull(ro1);
            ro1.SetParent(wearableObject.transform);

            // move the SomeWearableRootObject1 away somewhere to trigger remapping
            var wro1 = wearableObject.transform.Find("SomeWearableRootObject1");
            Assert.NotNull(wro1);
            wro1.SetParent(avatarObject.transform);

            // move the AvatarBlendshapeCube away somewhere to trigger remapping
            var abc = avatarObject.transform.Find("AvatarBlendshapeCube");
            Assert.NotNull(abc);
            abc.SetParent(wearableObject.transform);

            // move the WearableBlendshapeCube away somewhere to trigger remapping
            var wbc = wearableObject.transform.Find("WearableBlendshapeCube");
            Assert.NotNull(wbc);
            wbc.SetParent(avatarObject.transform);

            // move the dynbone away somewhere to trigger remapping
            var dynBone = wearableObject.transform.Find("Armature/Hips/MyDynBone");
            Assert.NotNull(dynBone);
            dynBone.SetParent(avatarObject.transform);

            var tuple = ag.GenerateWearAnimations();
            var enableClip = tuple.Item1;
            var disableClip = tuple.Item2;

            //
            // Enable clip
            //

            Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(enableClip).Length);
            var curveBindings = AnimationUtility.GetCurveBindings(enableClip);

            // avatar toggles
            // SomeRootObject1 is now at wearable root
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeRootObject1", "m_IsActive", 0.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 1.0f);

            // wearable toggles
            // SomeWearableRootObject1 is now at avatar root
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeWearableRootObject1", "m_IsActive", 1.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 0.0f);

            // blendshapes
            // AvatarBlendshapeCube is now at wearable root, and WearableBlendshapeCube at wearable root
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/AvatarBlendshapeCube", "blendShape.SomeKey", 90.0f);
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "WearableBlendshapeCube", "blendShape.SomeKey", 80.0f);

            // there is a dynamics at Wearable/Armature/Hips/MyDynBone
            // usually this will be grouped by cabinet applier to the root
            // MyDynBone is now at avatar root
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "MyDynBone", "m_IsActive", 1.0f);

            //
            // Disable clip
            //

            if (writeDefaults)
            {
                // for write defaults, disable clip should be all empty
                Assert.AreEqual(0, AnimationUtility.GetCurveBindings(disableClip).Length);
                Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(disableClip).Length);
            }
            else
            {
                // should be original values

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeWearableRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/AvatarBlendshapeCube", "blendShape.SomeKey", 20.0f);
                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "WearableBlendshapeCube", "blendShape.SomeKey", 30.0f);

                // turn off dynbone
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "MyDynBone", "m_IsActive", 0.0f);
            }
        }

        [Test]
        public void GenerateWearAnimations_WriteDefaults_NoRemapping_AllTests()
        {
            GenerateWearAnimations_NoRemapping_AllTests(true);
        }

        [Test]
        public void GenerateWearAnimations_NoWriteDefaults_NoRemapping_AllTests()
        {
            GenerateWearAnimations_NoRemapping_AllTests(false);
        }

        [Test]
        public void GenerateWearAnimations_WriteDefaults_WithRemapping_AllTests()
        {
            GenerateWearAnimations_WithRemapping_AllTests(true);
        }

        [Test]
        public void GenerateWearAnimations_NoWriteDefaults_WithRemapping_AllTests()
        {
            GenerateWearAnimations_WithRemapping_AllTests(false);
        }

        [Test]
        public void GenerateCustomizableToggleAnimations_NotGrandParent_ThrowsException()
        {
            var module = new CabinetAnimWearableModuleConfig();
            CreateAnimGen(module, true, out var avatarObject, out var wearableObject, out var report, out var ag);
            wearableObject.transform.SetParent(null);
            Assert.Throws(typeof(System.Exception), () => ag.GenerateCustomizableToggleAnimations());
        }

        private static CabinetAnimWearableModuleConfig.Customizable FindCustomizableByName(IEnumerable<CabinetAnimWearableModuleConfig.Customizable> customizables, string name)
        {
            foreach (var customizable in customizables)
            {
                if (customizable.name == name)
                {
                    return customizable;
                }
            }
            return null;
        }

        private void GenerateCustomizableToggleAnimations_AllTests(bool writeDefaults)
        {
            var module = CreateModule();
            CreateAnimGen(module, writeDefaults, out var avatarObject, out var wearableObject, out var report, out var ag);

            var dict = ag.GenerateCustomizableToggleAnimations();
            var toggleCustomizable = FindCustomizableByName(dict.Keys, "ToggleCustomizable");
            var tuple = dict[toggleCustomizable];
            var enableClip = tuple.Item1;
            var disableClip = tuple.Item2;

            //
            // Enable clip
            //

            Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(enableClip).Length);
            var curveBindings = AnimationUtility.GetCurveBindings(enableClip);

            // avatar toggles
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeRootObject1", "m_IsActive", 1.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 0.0f);

            // wearable toggles
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject1", "m_IsActive", 0.0f);
            AssertCurve(enableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 1.0f);

            // blendshapes
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "AvatarBlendshapeCube", "blendShape.SomeKey", 60.0f);
            AssertCurve(enableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/WearableBlendshapeCube", "blendShape.SomeKey", 70.0f);

            //
            // Disable clip
            //

            if (writeDefaults)
            {
                // for write defaults, disable clip should be all empty
                Assert.AreEqual(0, AnimationUtility.GetCurveBindings(disableClip).Length);
                Assert.AreEqual(0, AnimationUtility.GetObjectReferenceCurveBindings(disableClip).Length);
            }
            else
            {
                // should be original values

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "SomeRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject1", "m_IsActive", 1.0f);
                AssertCurve(disableClip, curveBindings, typeof(GameObject), "Wearable/SomeWearableRootObject2", "m_IsActive", 1.0f);

                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "AvatarBlendshapeCube", "blendShape.SomeKey", 20.0f);
                AssertCurve(disableClip, curveBindings, typeof(SkinnedMeshRenderer), "Wearable/WearableBlendshapeCube", "blendShape.SomeKey", 30.0f);
            }
        }

        [Test]
        public void GenerateCustomizableToggleAnimations_WriteDefaults_AllTests()
        {
            GenerateCustomizableToggleAnimations_AllTests(true);
        }

        [Test]
        public void GenerateCustomizableToggleAnimations_NoWriteDefaults_AllTests()
        {
            GenerateCustomizableToggleAnimations_AllTests(false);
        }

        private static bool IsCurveBindingExist(AnimationClip clip, System.Type type, string path, string propertyName)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.type == type && binding.path == path && binding.propertyName == propertyName)
                {
                    return true;
                }
            }
            return false;
        }

        [Test]
        public void GenerateCustomizableBlendshapeAnimations_AllTests()
        {
            // there are no difference with write defaults or not
            var module = CreateModule();
            CreateAnimGen(module, true, out var avatarObject, out var wearableObject, out var report, out var ag);

            var dict = ag.GenerateCustomizableBlendshapeAnimations();
            var blendshapeCustomizable = FindCustomizableByName(dict.Keys, "BlendshapeCustomizable");
            var clip = dict[blendshapeCustomizable];

            // we only tested whether the binding exist here
            Assert.True(IsCurveBindingExist(clip, typeof(SkinnedMeshRenderer), "Wearable/WearableBlendshapeCube", "blendShape.SomeKey"));
        }
    }
}
