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
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimatorStateBuilder
    {
        private readonly AnimatorState _state;
        private readonly AnimatorOptions _options;

        public AnimatorStateBuilder(AnimatorOptions options, AnimatorState state)
        {
            _state = state;
            _options = options;
        }

        public AnimatorStateTransitionBuilder AddTransition(AnimatorStateBuilder anotherState)
        {
            var transition = _state.AddTransition(anotherState.Build());

            transition.canTransitionToSelf = false;
            transition.duration = 0;
            transition.exitTime = 0;
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            //newTransition.interruptionSource = null;
            transition.isExit = false;
            transition.mute = false;
            transition.offset = 0;
            transition.orderedInterruption = true;
            transition.solo = false;
            transition.conditions = new AnimatorCondition[] { };

            return new AnimatorStateTransitionBuilder(transition);
        }

        // public AnimatorStateBuilder WithBehaviours(StateMachineBehaviour[] behaviours)
        // {
        //     _state.behaviours = behaviours;
        //     return this;
        // }

        internal AnimatorStateBuilder AddBehaviour(StateMachineBehaviour behaviour)
        {
            var array = new StateMachineBehaviour[_state.behaviours.Length + 1];
            _state.behaviours.CopyTo(array, 0);
            array[^1] = behaviour;
            _state.behaviours = array;
            return this;
        }

        // public AnimatorStateBuilder AddBehaviour<T>(out T behaviour) where T : StateMachineBehaviour
        // {
        //     behaviour = _state.AddStateMachineBehaviour<T>();
        //     return this;
        // }

        public AnimatorStateBuilder WithCycleOffset(float cycleOffset)
        {
            _state.cycleOffsetParameterActive = false;
            _state.cycleOffset = cycleOffset;
            return this;
        }

        public AnimatorStateBuilder WithCycleOffsetParameter(string cycleOffsetParameter)
        {
            _state.cycleOffsetParameterActive = true;
            _state.cycleOffsetParameter = cycleOffsetParameter;
            return this;
        }

        public AnimatorStateBuilder WithIkOnFeet(bool iKOnFeet)
        {
            _state.iKOnFeet = iKOnFeet;
            return this;
        }

        public AnimatorStateBuilder WithMirror(bool mirror)
        {
            _state.mirrorParameterActive = false;
            _state.mirror = mirror;
            return this;
        }

        public AnimatorStateBuilder WithMirrorParameter(string mirrorParameter)
        {
            _state.mirrorParameterActive = true;
            _state.mirrorParameter = mirrorParameter;
            return this;
        }

        public AnimatorStateBuilder WithSpeed(float speed)
        {
            _state.speedParameterActive = false;
            _state.speed = speed;
            return this;
        }

        public AnimatorStateBuilder WithSpeedParameter(string speedParameter)
        {
            _state.speedParameterActive = true;
            _state.speedParameter = speedParameter;
            return this;
        }

        public AnimatorStateBuilder WithMotionTime(AnimatorParameter param)
        {
            return WithMotionTime(param.Name);
        }

        public AnimatorStateBuilder WithMotionTime(string motionTimeParameter)
        {
            _state.timeParameterActive = true;
            _state.timeParameter = motionTimeParameter;
            return this;
        }

        public AnimatorStateBuilder WithoutMotionTime()
        {
            _state.timeParameterActive = false;
            _state.timeParameter = "";
            return this;
        }

        public AnimatorStateBuilder WithWriteDefaultValues(bool writeDefaultValues)
        {
            _state.writeDefaultValues = writeDefaultValues;
            return this;
        }

        public AnimatorStateBuilder WithMotion(Motion motion)
        {
            _state.motion = motion;
            return this;
        }

        public AnimationClipBuilder WithNewAnimation()
        {
            var clip = new AnimationClip();
            _state.motion = clip;
            _options.context.CreateUniqueAsset(clip, _state.name);

            // register this clip to allow remapping later on
            var store = _options.context.Feature<AnimationStore>();
            store.RegisterClip(clip, c => _state.motion = c);

            return new AnimationClipBuilder(_options, clip);
        }

        public AnimatorState Build()
        {
            return _state;
        }
    }
}
