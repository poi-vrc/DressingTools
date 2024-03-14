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
                ctrl.DriverType = DTSmartControl.SmartControlDriverType.MenuItem;
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
                ctrl.DriverType = DTSmartControl.SmartControlDriverType.MenuItem;
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
                ctrl.DriverType = DTSmartControl.SmartControlDriverType.MenuItem;

                composer.Compose(ctrl);

                Assert.True(z.TryGetComponent<DTMenuItem>(out _));
            }
        }

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

        private void ComposeBinaryTogglesTest(bool writeDefaults)
        {
            SetupEnv(out var root, out var options, out var ac);

            options.writeDefaults = writeDefaults;

            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<ParentConstraint>();

            var ctrl = a.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            ctrl.AsBinary()
                .Toggle(b, false)
                .Toggle(comp, true);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);

            var disabledClip = GetLayerStateClip(0, ac, "Disabled", writeDefaults);
            var enabledClip = GetLayerStateClip(0, ac, "Enabled", writeDefaults);
            var prepareDisabledClip = GetLayerStateClip(0, ac, "Prepare Disabled", writeDefaults);

            Assert.True(disabledClip.empty);

            if (writeDefaults)
            {
                Assert.True(prepareDisabledClip.empty);
            }
            else
            {
                Assert.True(HasToggleComponentCurve<Transform>(prepareDisabledClip, "B", true));
                Assert.True(HasToggleComponentCurve<ParentConstraint>(prepareDisabledClip, "C", true));
            }
            Assert.True(HasToggleComponentCurve<Transform>(enabledClip, "B", false));
            Assert.True(HasToggleComponentCurve<ParentConstraint>(enabledClip, "C", true));
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
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

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

            var disabledClip = GetLayerStateClip(0, ac, "Disabled", writeDefaults);
            var enabledClip = GetLayerStateClip(0, ac, "Enabled", writeDefaults);
            var prepareDisabledClip = GetLayerStateClip(0, ac, "Prepare Disabled", writeDefaults);

            Assert.True(disabledClip.empty);

            if (writeDefaults)
            {
                Assert.True(prepareDisabledClip.empty);
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
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "B/Cube", "blendShape.Key1", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "B/Cube", "blendShape.Key2", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "D/Cube", "blendShape.Key2", MagicBlendshapeFloat));
            // ignored object
            Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "C/Cube", "blendShape.Key2", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "B/Cube", "blendShape.Key3", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "C/Cube", "blendShape.Key3", MagicBlendshapeFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "D/Cube", "blendShape.Key3", MagicBlendshapeFloat));

            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "B/Cube", "material._Metallic", MagicFloat));
            Assert.True(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "C/Cube", "material._Metallic", MagicFloat));
            // avatar-wide ignored
            Assert.False(HasTogglePropertyCurve<SkinnedMeshRenderer>(enabledClip, "D/Cube", "material._Metallic", MagicFloat));
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
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            var magicFromValue = 25.0f;
            var magicToValue = 75.0f;
            ctrl.AsMotionTime()
                .AddPropertyGroup(ctrl.NewPropertyGroup()
                    .WithSelectedObjects(cubeB)
                    .ChangeProperty("blendShape.Key1", magicFromValue, magicToValue)
                );

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl);

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
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            var ctrl2 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            ctrl1.AsBinary()
                .CrossControlValueOnEnable(ctrl2, 0.0f)
                .CrossControlValueOnDisable(ctrl2, 1.0f);

            ctrl2.AsBinary()
                .CrossControlValueOnEnable(ctrl1, 1.0f)
                .CrossControlValueOnDisable(ctrl1, 0.0f);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl1);
            composer.Compose(ctrl2);

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
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            var ctrl2 = root.AddComponent<DTSmartControl>()
                .WithDriverType(DTSmartControl.SmartControlDriverType.AnimatorParameter);

            ctrl1.AsBinary()
                .CrossControlValueOnEnable(ctrl2, 0.0f);

            ctrl2.AsBinary()
                .CrossControlValueOnEnable(ctrl1, 0.0f);

            var composer = new SmartControlComposer(options, ac);
            composer.Compose(ctrl1);
            composer.Compose(ctrl2);

            var enabledState1 = GetLayerState(0, ac, "Enabled", options.writeDefaults);
            Assert.True(ContainsVRCAvatarParameterDriver(enabledState1, ctrl2.AnimatorConfig.ParameterName, 0.0f));

            var enabledState2 = GetLayerState(1, ac, "Enabled", options.writeDefaults);
            Assert.True(ContainsVRCAvatarParameterDriver(enabledState2, ctrl1.AnimatorConfig.ParameterName, 0.0f));
        }
#endif
    }
}
