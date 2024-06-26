﻿/*
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
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Tests.Animations
{
    internal class SmartControlComposerTest : AnimationsTestBase
    {
        private const float MagicFloat = 0.75f;
        private const float MagicBlendshapeFloat = 75.0f;

        [Test]
        public void DuplicateControlTest()
        {
            SetupEnv(out var root, out var options, out var ac);
            var ctrl = root.AddComponent<DTSmartControl>();

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);
            Assert.AreEqual(1, ac.layers.Length);
            composer.Compose(ctrl);
            Assert.AreEqual(1, ac.layers.Length);
        }

        [Test]
        public void GenerateParameterNameTest()
        {
            SetupEnv(out var root, out var options, out var ac);
            var ctrl = root.AddComponent<DTSmartControl>();

            ctrl.AnimatorConfig.ParameterName = null;
            Assert.Null(ctrl.AnimatorConfig.ParameterName);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);

            Assert.False(string.IsNullOrEmpty(ctrl.AnimatorConfig.ParameterName));
        }

        [Test]
        public void MenuItemDriverTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            // Button/Toggle
            {
                var a = CreateGameObject("A", root.transform);
                var menuItem = a.AddComponent<DTMenuItem>();
                var ctrl = a.AddComponent<DTSmartControl>();
                ctrl.DriverType = DTSmartControl.SCDriverType.MenuItem;
                ctrl.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Button;

                composer.Compose(ctrl);

                Assert.AreEqual(ctrl.MenuItemDriverConfig.ItemType, menuItem.Type);
                Assert.AreEqual(DTMenuItem.ItemController.ControllerType.AnimatorParameter, menuItem.Controller.Type);
                Assert.AreEqual(ctrl.AnimatorConfig.ParameterName, menuItem.Controller.AnimatorParameterName);
            }

            // Radial
            {
                var b = CreateGameObject("B", root.transform);
                var menuItem = b.AddComponent<DTMenuItem>();
                var ctrl = b.AddComponent<DTSmartControl>();
                ctrl.DriverType = DTSmartControl.SCDriverType.MenuItem;
                ctrl.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Radial;

                composer.Compose(ctrl);

                Assert.AreEqual(ctrl.MenuItemDriverConfig.ItemType, menuItem.Type);
                Assert.AreEqual(1, menuItem.SubControllers.Length);
                Assert.AreEqual(DTMenuItem.ItemController.ControllerType.AnimatorParameter, menuItem.SubControllers[0].Type);
                Assert.AreEqual(ctrl.AnimatorConfig.ParameterName, menuItem.SubControllers[0].AnimatorParameterName);
            }

            // create a new menuitem component if not exist
            {
                var z = CreateGameObject("Z", root.transform);
                var ctrl = z.AddComponent<DTSmartControl>();
                ctrl.DriverType = DTSmartControl.SCDriverType.MenuItem;

                composer.Compose(ctrl);

                Assert.True(z.TryGetComponent<DTMenuItem>(out _));
            }
        }

        private void AssertParameterSlotState(AnimatorController ac, AnimatorOptions options, DTParameterSlot slot, DTSmartControl sc, DTSmartControl another1, DTSmartControl another2)
        {
            var entryState = GetLayerState(0, ac, "Entry", options.writeDefaults);
            Assert.AreEqual(3, entryState.transitions.Length);
            Assert.True(entryState.transitions.Where(t =>
                t.conditions.Where(c =>
                    c.parameter == slot.ParameterName &&
                    c.mode == AnimatorConditionMode.Equals &&
                    c.threshold == sc.ParameterSlotConfig.MappedValue
                ).Count() == 1
            ).Count() == 1);

            var enabledState = GetLayerState(0, ac, $"{sc.name} Enabled", options.writeDefaults);
            Assert.AreEqual(1, enabledState.transitions.Length);
            Assert.True(enabledState.transitions.Where(t =>
                t.conditions.Where(c =>
                    c.parameter == slot.ParameterName &&
                    c.mode == AnimatorConditionMode.NotEqual &&
                    c.threshold == sc.ParameterSlotConfig.MappedValue
                ).Count() == 1
            ).Count() == 1);

            var prepareDisabledState = GetLayerState(0, ac, $"{sc.name} Prepare Disabled", options.writeDefaults);
            // expects to have 3 transitions: Entry, and the another two SC
            Assert.AreEqual(3, prepareDisabledState.transitions.Length);
            Assert.True(prepareDisabledState.transitions.Where(t =>
                t.conditions.Where(c =>
                    c.parameter == slot.ParameterName &&
                    c.mode == AnimatorConditionMode.Equals &&
                    c.threshold == another1.ParameterSlotConfig.MappedValue
                ).Count() == 1
            ).Count() == 1);
            Assert.True(prepareDisabledState.transitions.Where(t =>
                t.conditions.Where(c =>
                    c.parameter == slot.ParameterName &&
                    c.mode == AnimatorConditionMode.Equals &&
                    c.threshold == another2.ParameterSlotConfig.MappedValue
                ).Count() == 1
            ).Count() == 1);
            var notEqualsTransitionQuery = prepareDisabledState.transitions.Where(t =>
                t.conditions.Where(c =>
                    c.parameter == slot.ParameterName &&
                    c.mode == AnimatorConditionMode.NotEqual &&
                    c.threshold == sc.ParameterSlotConfig.MappedValue
                ).Count() == 1
            );
            Assert.True(notEqualsTransitionQuery.Count() == 1);
            // expects the NotEquals transitions to be the last one
            var notEqualsTransition = notEqualsTransitionQuery.FirstOrDefault();
            Assert.NotNull(notEqualsTransition);
            Assert.AreEqual(notEqualsTransition, prepareDisabledState.transitions[prepareDisabledState.transitions.Length - 1]);
        }

        [Test]
        public void ParameterSlotDriverTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var slotGo = CreateGameObject("Slot", root.transform);
            var slot = slotGo.AddComponent<DTParameterSlot>();

            var sc1Go = CreateGameObject("SC1", root.transform);
            var sc1 = sc1Go.AddComponent<DTSmartControl>();
            sc1.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            sc1.ParameterSlotConfig.ParameterSlot = slot;
            sc1.ParameterSlotConfig.MappedValue = 0;
            sc1.ParameterSlotConfig.GenerateMenuItem = false;

            var sc2Go = CreateGameObject("SC2", root.transform);
            var sc2 = sc2Go.AddComponent<DTSmartControl>();
            sc2.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            sc2.ParameterSlotConfig.ParameterSlot = slot;
            sc2.ParameterSlotConfig.MappedValue = 1;
            sc2.ParameterSlotConfig.GenerateMenuItem = true;

            var sc3Go = CreateGameObject("SC3", root.transform);
            var sc3 = sc3Go.AddComponent<DTSmartControl>();
            sc3.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            sc3.ParameterSlotConfig.ParameterSlot = slot;
            sc3.ParameterSlotConfig.MappedValue = 2;

            composer.Compose(sc1);
            composer.Compose(sc2);
            composer.Compose(sc3);
            composer.Finish();

            Assert.False(sc1Go.TryGetComponent<DTMenuItem>(out _));
            Assert.True(sc2Go.TryGetComponent<DTMenuItem>(out _));
            Assert.False(sc3Go.TryGetComponent<DTMenuItem>(out _));

            AssertParameterSlotState(ac, options, slot, sc1, sc2, sc3);
            AssertParameterSlotState(ac, options, slot, sc2, sc1, sc3);
            AssertParameterSlotState(ac, options, slot, sc3, sc1, sc2);
        }

#if DT_VRCSDK3A
        [Test]
        public void VRCPhysBoneDriver_GeneratePrefixTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var obj = CreateGameObject("A", root.transform);
            var ctrl = obj.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.VRCPhysBone;
            ctrl.VRCPhysBoneDriverConfig.Condition = DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.Grabbed;
            ctrl.VRCPhysBoneDriverConfig.Source = DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.None;

            composer.Compose(ctrl);
            composer.Finish();

            Assert.True(string.IsNullOrEmpty(ctrl.AnimatorConfig.ParameterName));
            Assert.False(string.IsNullOrEmpty(ctrl.VRCPhysBoneDriverConfig.ParameterPrefix));
        }

        [Test]
        public void VRCPhysBoneDriver_SrcNone_CondGrabbedOrPosedTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var obj = CreateGameObject("A", root.transform);
            var pb = obj.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
            var ctrl = obj.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.VRCPhysBone;
            ctrl.VRCPhysBoneDriverConfig.VRCPhysBone = pb;
            ctrl.VRCPhysBoneDriverConfig.Condition = DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.GrabbedOrPosed;
            ctrl.VRCPhysBoneDriverConfig.Source = DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.None;

            composer.Compose(ctrl);
            composer.Finish();

            var disabledState = GetLayerState(0, ac, "Disabled", options.writeDefaults);
            // two separate transitions with one condition
            Assert.AreEqual(2, disabledState.transitions.Length);
            Assert.AreEqual(1, disabledState.transitions[0].conditions.Length);
            Assert.True(disabledState.transitions[0].conditions[0].mode == AnimatorConditionMode.If);
            Assert.AreEqual(1, disabledState.transitions[1].conditions.Length);
            Assert.True(disabledState.transitions[1].conditions[0].mode == AnimatorConditionMode.If);

            var enabledState = GetLayerState(0, ac, "Enabled", options.writeDefaults);
            // one transition with two conditions
            Assert.AreEqual(1, enabledState.transitions.Length);
            Assert.AreEqual(2, enabledState.transitions[0].conditions.Length);
            Assert.True(enabledState.transitions[0].conditions[0].mode == AnimatorConditionMode.IfNot);
            Assert.True(enabledState.transitions[0].conditions[1].mode == AnimatorConditionMode.IfNot);

            var prepareDisabledState = GetLayerState(0, ac, "Prepare Disabled", options.writeDefaults);
            // one transition with two conditions
            Assert.AreEqual(1, prepareDisabledState.transitions.Length);
            Assert.AreEqual(2, prepareDisabledState.transitions[0].conditions.Length);
            Assert.True(prepareDisabledState.transitions[0].conditions[0].mode == AnimatorConditionMode.IfNot);
            Assert.True(prepareDisabledState.transitions[0].conditions[1].mode == AnimatorConditionMode.IfNot);
        }

        [Test]
        public void VRCPhysBoneDriver_SrcAngle_CondNoneTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var obj = CreateGameObject("A", root.transform);
            var pb = obj.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
            var ctrl = obj.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.VRCPhysBone;
            ctrl.VRCPhysBoneDriverConfig.VRCPhysBone = pb;
            ctrl.VRCPhysBoneDriverConfig.Condition = DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.None;
            ctrl.VRCPhysBoneDriverConfig.Source = DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.Angle;

            composer.Compose(ctrl);
            composer.Finish();

            var motionTimeState = GetLayerState(0, ac, "Motion Time", options.writeDefaults);
            Assert.AreEqual(motionTimeState.timeParameter, $"{pb.parameter}_Angle");
        }

        [Test]
        public void VRCPhysBoneDriver_SrcAngle_CondGrabbedOrPosedTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var obj = CreateGameObject("A", root.transform);
            var pb = obj.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
            var ctrl = obj.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.VRCPhysBone;
            ctrl.VRCPhysBoneDriverConfig.VRCPhysBone = pb;
            ctrl.VRCPhysBoneDriverConfig.Condition = DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.GrabbedOrPosed;
            ctrl.VRCPhysBoneDriverConfig.Source = DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.Angle;

            composer.Compose(ctrl);
            composer.Finish();

            var disabledState = GetLayerState(0, ac, "Disabled", options.writeDefaults);
            // two separate transitions with one condition
            Assert.AreEqual(2, disabledState.transitions.Length);
            Assert.AreEqual(1, disabledState.transitions[0].conditions.Length);
            Assert.True(disabledState.transitions[0].conditions[0].mode == AnimatorConditionMode.If);
            Assert.AreEqual(1, disabledState.transitions[1].conditions.Length);
            Assert.True(disabledState.transitions[1].conditions[0].mode == AnimatorConditionMode.If);

            var motionTimeState = GetLayerState(0, ac, "Motion Time", options.writeDefaults);
            Assert.AreEqual(motionTimeState.timeParameter, $"{pb.parameter}_Angle");
            // one transition with two conditions
            Assert.AreEqual(1, motionTimeState.transitions.Length);
            Assert.AreEqual(2, motionTimeState.transitions[0].conditions.Length);
            Assert.True(motionTimeState.transitions[0].conditions[0].mode == AnimatorConditionMode.IfNot);
            Assert.True(motionTimeState.transitions[0].conditions[1].mode == AnimatorConditionMode.IfNot);
        }
#endif

        [Test]
        public void AddParameterConfigTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var composer = new SmartControlComposer(options, ac);

            var a = CreateGameObject("A", root.transform);
            var ctrl = a.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = "MyParam";
            ctrl.AnimatorConfig.NetworkSynced = true;
            ctrl.AnimatorConfig.Saved = false;

            composer.Compose(ctrl);
            composer.Finish();

            Assert.True(ctrl.TryGetComponent<DTAnimatorParameters>(out var animParams));
            var ap = animParams.Configs.Where(p => p.ParameterName == "MyParam").FirstOrDefault();
            Assert.NotNull(ap);
            Assert.True(ap.NetworkSynced);
            Assert.False(ap.Saved);
        }

        private static AnimatorState GetLayerState(int layerIndex, AnimatorController ac, string stateName, bool expectedWriteDefaults)
        {
            Assert.Less(layerIndex, ac.layers.Length);
            var state = ac.layers[layerIndex].stateMachine.states.Where(s => s.state.name == stateName).FirstOrDefault();
            Assert.NotNull(state);
            Assert.AreEqual(expectedWriteDefaults, state.state.writeDefaultValues);
            return state.state;
        }

        private static AnimationClip GetLayerStateClip(int layerIndex, AnimatorController ac, string stateName, bool expectedWriteDefaults)
        {
            var state = GetLayerState(layerIndex, ac, stateName, expectedWriteDefaults);
            Assert.IsInstanceOf(typeof(AnimationClip), state.motion);
            return (AnimationClip)state.motion;
        }

        public bool HasToggleComponentCurve<T>(AnimationClip clip, string path, bool enabled) where T : Component
        {
            var curve = AnimationCurve.Constant(0.0f, 0.0f, enabled ? 1.0f : 0.0f);
            return typeof(T) == typeof(Transform) ?
                HasEditorCurve(clip, path, typeof(GameObject), "m_IsActive", curve) :
                HasEditorCurve(clip, path, typeof(T), "m_Enabled", curve);
        }

        public bool HasTogglePropertyCurve<T>(AnimationClip clip, string path, string property, float value) where T : Component
        {
            var curve = AnimationCurve.Constant(0.0f, 0.0f, value);
            return HasEditorCurve(clip, path, typeof(T), property, curve);
        }

        private void AssertComposeBinaryTogglesEnabledClip(AnimationClip clip)
        {
            Assert.True(HasToggleComponentCurve<Transform>(clip, "B", false));
            Assert.True(HasToggleComponentCurve<ParentConstraint>(clip, "C", true));
        }

        private void ComposeBinaryTogglesTest(bool writeDefaults)
        {
            SetupEnv(out var root, out var options, out var ac);

            options.writeDefaults = writeDefaults;

            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<ParentConstraint>();

            var ctrl = a.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            ctrl.AsBinary()
                .Toggle(b, false)
                .Toggle(comp, true);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);
            composer.Finish();

            var disabledClip = GetLayerStateClip(0, ac, "Disabled", writeDefaults);
            var enabledClip = GetLayerStateClip(0, ac, "Enabled", writeDefaults);
            var prepareDisabledClip = GetLayerStateClip(0, ac, "Prepare Disabled", writeDefaults);

            Assert.True(disabledClip.empty);

            if (writeDefaults)
            {
                // expects the same as enabled clip
                AssertComposeBinaryTogglesEnabledClip(prepareDisabledClip);
            }
            else
            {
                Assert.True(HasToggleComponentCurve<Transform>(prepareDisabledClip, "B", true));
                Assert.True(HasToggleComponentCurve<ParentConstraint>(prepareDisabledClip, "C", true));
            }
            AssertComposeBinaryTogglesEnabledClip(enabledClip);
        }

        [Test]
        public void ComposeBinaryTogglesTest_WriteDefaultsOn()
        {
            ComposeBinaryTogglesTest(true);
        }

        [Test]
        public void ComposeBinaryTogglesTest_WriteDefaultsOff()
        {
            ComposeBinaryTogglesTest(false);
        }

        private void AssertComposeBinaryPropertyEnabledClip(AnimationClip clip)
        {
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "B/Cube", "blendShape.Key1", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "B/Cube", "blendShape.Key2", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "D/Cube", "blendShape.Key2", MagicBlendshapeFloat));
            // ignored object
            Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "C/Cube", "blendShape.Key2", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "B/Cube", "blendShape.Key3", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "C/Cube", "blendShape.Key3", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "D/Cube", "blendShape.Key3", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "B/Cube", "material._Metallic", MagicFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "C/Cube", "material._Metallic", MagicFloat));
            // avatar-wide ignored
            Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(clip, "D/Cube", "material._Metallic", MagicFloat));
        }

        private void ComposeBinaryPropertyTest(bool writeDefaults)
        {
            SetupEnv(out var root, out var options, out var ac);

            options.writeDefaults = writeDefaults;

            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var cubeB = InstantiateEditorTestPrefab("TestCubePrefab.prefab", b.transform);
            cubeB.name = "Cube";
            var c = CreateGameObject("C", root.transform);
            var cubeC = InstantiateEditorTestPrefab("TestCubePrefab.prefab", c.transform);
            cubeC.name = "Cube";
            var d = CreateGameObject("D", root.transform);
            var cubeD = InstantiateEditorTestPrefab("TestCubePrefab.prefab", d.transform);
            cubeD.name = "Cube";

            var ctrl = a.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            ctrl.AsBinary()
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .WithSelectedObjects(cubeB)
                    .ChangeProperty("blendShape.Key1", MagicBlendshapeFloat)
                )
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .SearchIn(root.transform)
                    .WithIgnoredObjects(cubeC)
                    .ChangeProperty("blendShape.Key2", MagicBlendshapeFloat)
                )
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .WithAvatarWide()
                    .ChangeProperty("blendShape.Key3", MagicBlendshapeFloat)
                )
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .WithAvatarWideAndIgnore(cubeD)
                    .ChangeProperty("material._Metallic", MagicFloat)
                );

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);
            composer.Finish();

            var disabledClip = GetLayerStateClip(0, ac, "Disabled", writeDefaults);
            var enabledClip = GetLayerStateClip(0, ac, "Enabled", writeDefaults);
            var prepareDisabledClip = GetLayerStateClip(0, ac, "Prepare Disabled", writeDefaults);

            Assert.True(disabledClip.empty);

            if (writeDefaults)
            {
                // expects the same as enabled clip
                AssertComposeBinaryPropertyEnabledClip(prepareDisabledClip);
            }
            else
            {
                // existing values from prefab
                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "B/Cube", "blendShape.Key1", 60.0f));

                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "B/Cube", "blendShape.Key2", 50.0f));
                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "D/Cube", "blendShape.Key2", 50.0f));
                // ignored object
                Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "C/Cube", "blendShape.Key2", 50.0f));

                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "B/Cube", "blendShape.Key3", 40.0f));
                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "C/Cube", "blendShape.Key3", 40.0f));
                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "D/Cube", "blendShape.Key3", 40.0f));

                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "B/Cube", "material._Metallic", 0.65f));
                Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "C/Cube", "material._Metallic", 0.65f));
                // avatar-wide ignored
                Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(prepareDisabledClip, "D/Cube", "material._Metallic", 0.65f));
            }
            AssertComposeBinaryPropertyEnabledClip(enabledClip);
        }

        [Test]
        public void ComposeBinaryPropertyTest_WriteDefaultsOn()
        {
            ComposeBinaryPropertyTest(true);
        }

        [Test]
        public void ComposeBinaryPropertyTest_WriteDefaultsOff()
        {
            ComposeBinaryPropertyTest(false);
        }

        public bool HasPropertyFromToCurve<T>(AnimationClip clip, string path, string property, float from, float to) where T : Component
        {
            var curve = AnimationCurve.Linear(0.0f, from, 1.0f, to);
            return HasEditorCurve(clip, path, typeof(T), property, curve);
        }

        private void ComposeMotionTimeTest(bool writeDefaults)
        {
            SetupEnv(out var root, out var options, out var ac);

            options.writeDefaults = writeDefaults;

            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var cubeB = InstantiateEditorTestPrefab("TestCubePrefab.prefab", b.transform);
            cubeB.name = "Cube";

            var ctrl = a.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            var magicFromValue = 25.0f;
            var magicToValue = 75.0f;
            ctrl.AsMotionTime()
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .WithSelectedObjects(cubeB)
                    .ChangeProperty("blendShape.Key1", magicFromValue, magicToValue)
                );

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);
            composer.Finish();

            var state = GetLayerState(0, ac, "Motion Time", writeDefaults);
            Assert.IsInstanceOf(typeof(AnimationClip), state.motion);
            Assert.True(state.timeParameterActive);
            Assert.AreEqual(ctrl.AnimatorConfig.ParameterName, state.timeParameter);

            var clip = (AnimationClip)state.motion;
            Assert.True(HasPropertyFromToCurve<SkinnedMeshRenderer>(clip, "B/Cube", "blendShape.Key1", magicFromValue, magicToValue));
        }

        [Test]
        public void ComposeMotionTimeTest_WriteDefaultsOn()
        {
            ComposeMotionTimeTest(true);
        }

        [Test]
        public void ComposeMotionTimeTest_WriteDefaultsOff()
        {
            ComposeMotionTimeTest(false);
        }

        [Test]
        public void CrossControlsCycleTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var ctrl1 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            var ctrl2 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            ctrl1.AsBinary()
                .CrossControlValueOnEnable(ctrl2, 0.0f)
                .CrossControlValueOnDisable(ctrl2, 1.0f);

            ctrl2.AsBinary()
                .CrossControlValueOnEnable(ctrl1, 1.0f)
                .CrossControlValueOnDisable(ctrl1, 0.0f);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl1);
            composer.Compose(ctrl2);
            composer.Finish();

            Assert.True(options.context.Report.HasLogType(DressingFramework.Logging.LogType.Error));
        }

#if DT_VRCSDK3A
        private static bool ContainsVRCAvatarParameterDriver(AnimatorState state, string parameter, float value)
        {
            return state.behaviours.Where(b =>
             {
                 if (!(b is VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver driver))
                 {
                     return false;
                 }
                 return driver.parameters.Where(p =>
                     p.name == parameter &&
                     p.value == value
                     ).Count() > 0;
             }).Count() > 0;
        }

        [Test]
        public void CrossControlsTest()
        {
            SetupEnv(out var root, out var options, out var ac);

            var ctrl1 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            var ctrl2 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SCDriverType.AnimatorParameter);

            ctrl1.AsBinary()
                .CrossControlValueOnEnable(ctrl2, 0.0f);

            ctrl2.AsBinary()
                .CrossControlValueOnEnable(ctrl1, 0.0f);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl1);
            composer.Compose(ctrl2);
            composer.Finish();

            var enabledState1 = GetLayerState(0, ac, "Enabled", options.writeDefaults);
            Assert.True(ContainsVRCAvatarParameterDriver(enabledState1, ctrl2.AnimatorConfig.ParameterName, 0.0f));

            var enabledState2 = GetLayerState(1, ac, "Enabled", options.writeDefaults);
            Assert.True(ContainsVRCAvatarParameterDriver(enabledState2, ctrl1.AnimatorConfig.ParameterName, 0.0f));
        }
#endif
    }
}
