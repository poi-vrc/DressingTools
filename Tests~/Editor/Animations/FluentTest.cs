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

using System;
using System.Linq;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Animations.Fluent;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Animations
{
    internal class FluentTest : AnimationsTestBase
    {
        private const float MagicFloat = 0.75f;
        private const int MagicInt = 127;

        [Test]
        public void ClipSetCurveTest()
        {
            var clip = new AnimationClip();

            SetupEnv(out var root, out var options, out _);
            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var builder = new AnimationClipBuilder(options, clip);

            var someCurve = AnimationCurve.Constant(0.0f, 0.0f, 0.0f);

            builder.SetCurve("SomePath", typeof(Animator), "SomeProperty", someCurve);
            Assert.True(HasEditorCurve(clip, "SomePath", typeof(Animator), "SomeProperty", someCurve));

            builder.SetCurve(a.transform, typeof(Animator), "SomeProperty", someCurve);
            Assert.True(HasEditorCurve(clip, "A", typeof(Animator), "SomeProperty", someCurve));

            var objRefFrames = new ObjectReferenceKeyframe[] {
                    new ObjectReferenceKeyframe() { time = 1.0f, value = b }
                };
            builder.SetCurve(b.transform, typeof(Animator), "SomeProperty", objRefFrames);
            Assert.True(HasObjRefCurve(clip, "B", typeof(Animator), "SomeProperty", objRefFrames));
        }

        [Test]
        public void ClipToggleTest()
        {
            var clip = new AnimationClip();

            SetupEnv(out var root, out var options, out _);
            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", root.transform);
            var builder = new AnimationClipBuilder(options, clip);

            var expectedDisabledCurve = AnimationCurve.Constant(0.0f, 0.0f, 0.0f);
            var expectedEnabledCurve = AnimationCurve.Constant(0.0f, 0.0f, 1.0f);

            var comp1 = a.AddComponent<Animator>();
            builder.Toggle(comp1, true);
            Assert.True(HasEditorCurve(clip, "A", typeof(Animator), "m_Enabled", expectedEnabledCurve));

            var comp2 = b.AddComponent<Animator>();
            builder.Toggle(comp2, false);
            Assert.True(HasEditorCurve(clip, "B", typeof(Animator), "m_Enabled", expectedDisabledCurve));

            builder.Toggle(a, true);
            Assert.True(HasEditorCurve(clip, "A", typeof(GameObject), "m_IsActive", expectedEnabledCurve));

            builder.Toggle(b, false);
            Assert.True(HasEditorCurve(clip, "B", typeof(GameObject), "m_IsActive", expectedDisabledCurve));
        }

        [Test]
        public void ClipBlendshapeTest()
        {
            var clip = new AnimationClip();

            SetupEnv(out var root, out var options, out _);
            var a = CreateGameObject("A", root.transform);
            var smr = a.AddComponent<SkinnedMeshRenderer>();
            var builder = new AnimationClipBuilder(options, clip);

            var expectedExactCurve = AnimationCurve.Constant(0.0f, 0.0f, 50.0f);
            var expectedCustomCurve = AnimationCurve.Linear(0.0f, 100.0f, 0.0f, 1.0f);

            builder.Blendshape(smr, "SomeBlendshape1", 50.0f);
            Assert.True(HasEditorCurve(clip, "A", typeof(SkinnedMeshRenderer), "blendShape.SomeBlendshape1", expectedExactCurve));

            builder.Blendshape(smr, "SomeBlendshape2", expectedCustomCurve);
            Assert.True(HasEditorCurve(clip, "A", typeof(SkinnedMeshRenderer), "blendShape.SomeBlendshape2", expectedCustomCurve));
        }

        [Test]
        public void ClipBuildTest()
        {
            var clip = new AnimationClip();

            SetupEnv(out var root, out var options, out _);
            var builder = new AnimationClipBuilder(options, clip);

            Assert.AreEqual(clip, builder.Build());
        }

        private bool HasCondition(AnimatorStateTransition transition, AnimatorConditionMode mode, string parameterName, float? threshold = null)
        {
            return transition.conditions
                .Where(c =>
                    c.mode == mode &&
                    c.parameter == parameterName &&
                    (threshold == null || threshold.Value == c.threshold))
                .Count() > 0;
        }

        [Test]
        public void TransitionTest()
        {
            var transition = new AnimatorStateTransition();
            var builder = new AnimatorStateTransitionBuilder(transition);

            builder.If("BoolParam1");
            Assert.True(HasCondition(transition, AnimatorConditionMode.If, "BoolParam1"));

            builder.If(new AnimatorParameter(AnimatorControllerParameterType.Bool, "BoolParam2"));
            Assert.True(HasCondition(transition, AnimatorConditionMode.If, "BoolParam2"));

            builder.IfNot("BoolParam3");
            Assert.True(HasCondition(transition, AnimatorConditionMode.IfNot, "BoolParam3"));

            builder.IfNot(new AnimatorParameter(AnimatorControllerParameterType.Bool, "BoolParam4"));
            Assert.True(HasCondition(transition, AnimatorConditionMode.IfNot, "BoolParam4"));

            builder.Equals("FloatParam1", MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Equals, "FloatParam1", MagicFloat));

            builder.Equals(new AnimatorParameter(AnimatorControllerParameterType.Float, "FloatParam2"), MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Equals, "FloatParam2", MagicFloat));

            builder.NotEquals("FloatParam3", MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.NotEqual, "FloatParam3", MagicFloat));

            builder.NotEquals(new AnimatorParameter(AnimatorControllerParameterType.Float, "FloatParam4"), MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.NotEqual, "FloatParam4", MagicFloat));

            builder.Greater("FloatParam5", MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Greater, "FloatParam5", MagicFloat));

            builder.Greater(new AnimatorParameter(AnimatorControllerParameterType.Float, "FloatParam5"), MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Greater, "FloatParam5", MagicFloat));

            builder.Less("FloatParam6", MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Less, "FloatParam6", MagicFloat));

            builder.Less(new AnimatorParameter(AnimatorControllerParameterType.Float, "FloatParam6"), MagicFloat);
            Assert.True(HasCondition(transition, AnimatorConditionMode.Less, "FloatParam6", MagicFloat));

            Assert.AreEqual(transition, builder.Build());
        }

        [Test]
        public void StateTest()
        {
            var state = new AnimatorState();

            SetupEnv(out var root, out var options, out _);
            var builder = new AnimatorStateBuilder(options, state);

            var anotherState = new AnimatorState();
            var transitionBuilder = builder.AddTransition(new AnimatorStateBuilder(options, anotherState));
            Assert.Contains(transitionBuilder.Build(), state.transitions);

            // TODO: fix this test
            // builder.AddBehaviour<DummyStateBehaviour>(out var dummyBehaviour);
            // Assert.Contains(dummyBehaviour, state.behaviours);

            builder.WithCycleOffset(MagicFloat);
            Assert.False(state.cycleOffsetParameterActive);
            Assert.AreEqual(MagicFloat, state.cycleOffset);

            builder.WithCycleOffsetParameter("SomeParam1");
            Assert.True(state.cycleOffsetParameterActive);
            Assert.AreEqual("SomeParam1", state.cycleOffsetParameter);

            builder.WithSpeed(MagicFloat);
            Assert.False(state.speedParameterActive);
            Assert.AreEqual(MagicFloat, state.speed);

            builder.WithSpeedParameter("SomeParam2");
            Assert.True(state.speedParameterActive);
            Assert.AreEqual("SomeParam2", state.speedParameter);

            var expectedTrue = true;

            builder.WithIkOnFeet(expectedTrue);
            Assert.AreEqual(expectedTrue, state.iKOnFeet);

            builder.WithMirror(expectedTrue);
            Assert.False(state.mirrorParameterActive);
            Assert.AreEqual(expectedTrue, state.mirror);

            builder.WithMirrorParameter("SomeParam3");
            Assert.True(state.mirrorParameterActive);
            Assert.AreEqual("SomeParam3", state.mirrorParameter);

            builder.WithMotionTime("SomeParam4");
            Assert.True(state.timeParameterActive);
            Assert.AreEqual("SomeParam4", state.timeParameter);

            builder.WithoutMotionTime();
            Assert.False(state.timeParameterActive);
            Assert.AreEqual("", state.timeParameter);

            var expectedFalse = false;

            builder.WithWriteDefaultValues(expectedFalse);
            Assert.AreEqual(expectedFalse, state.writeDefaultValues);

            var clip = new AnimationClip();

            builder.WithMotion(clip);
            Assert.AreEqual(clip, state.motion);

            var clipBuilder = builder.WithNewAnimation();
            Assert.AreEqual(clipBuilder.Build(), state.motion);
        }

        [Test]
        public void LayerTest()
        {
            var layer = new AnimatorControllerLayer()
            {
                stateMachine = new AnimatorStateMachine()
            };

            SetupEnv(out var root, out var options, out _);
            var builder = new AnimatorLayerBuilder(options, layer);

            var expectedState1Name = "Abc";
            var state1 = builder.NewState(expectedState1Name);
            Assert.True(layer.stateMachine.states.Where(s => s.state.name == expectedState1Name).Count() > 0);

            var expectedState2Name = "Def";
            var expectedState2Pos = new Vector3(1, 2, 3);
            var state2 = builder.NewState(expectedState2Name, expectedState2Pos);
            Assert.True(layer.stateMachine.states.Where(s => s.state.name == expectedState2Name && s.position == expectedState2Pos).Count() > 0);

            Assert.AreEqual(state1.Build(), layer.stateMachine.defaultState);
            builder.WithDefaultState(state2);
            Assert.AreEqual(state2.Build(), layer.stateMachine.defaultState);

            Assert.AreEqual(builder.Build(), layer);
        }

        [Test]
        public void AnimatorTest()
        {
            SetupEnv(out var root, out var options, out var ctrl);
            var builder = new AnimatorBuilder(options, ctrl);

            builder.IntParameter("Param1", MagicInt);
            Assert.True(ctrl.parameters.Where(p =>
                p.name == "Param1" &&
                p.type == AnimatorControllerParameterType.Int &&
                p.defaultInt == MagicInt
                ).Count() > 0);

            builder.FloatParameter("Param2", MagicFloat);
            Assert.True(ctrl.parameters.Where(p =>
                p.name == "Param2" &&
                p.type == AnimatorControllerParameterType.Float &&
                p.defaultFloat == MagicFloat
                ).Count() > 0);

            builder.BoolParameter("Param3", true);
            Assert.True(ctrl.parameters.Where(p =>
                p.name == "Param3" &&
                p.type == AnimatorControllerParameterType.Bool &&
                p.defaultBool == true
                ).Count() > 0);

            var layerBuilder = builder.NewLayer("SomeLayer");
            var layer = layerBuilder.Build();
            Assert.AreEqual("SomeLayer", layer.name);
            Assert.True(ctrl.layers.Where(acl => acl.name == layer.name).Count() > 0);

            Assert.AreEqual(builder.Build(), ctrl);
        }
    }
}
