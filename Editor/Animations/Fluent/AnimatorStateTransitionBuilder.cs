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

using UnityEditor.Animations;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimatorStateTransitionBuilder
    {
        private readonly AnimatorStateTransition _stateTransition;

        public AnimatorStateTransitionBuilder(AnimatorStateTransition stateTransition)
        {
            _stateTransition = stateTransition;
        }

        private AnimatorStateTransitionBuilder AddCondition(AnimatorConditionMode mode, float threshold, string parameterName)
        {
            _stateTransition.AddCondition(mode, threshold, parameterName);
            return this;
        }

        public AnimatorStateTransitionBuilder If(AnimatorParameter param)
        {
            return If(param.Name);
        }

        public AnimatorStateTransitionBuilder If(string parameterName)
        {
            return AddCondition(AnimatorConditionMode.If, 0, parameterName);
        }

        public AnimatorStateTransitionBuilder IfNot(AnimatorParameter param)
        {
            return IfNot(param.Name);
        }

        public AnimatorStateTransitionBuilder IfNot(string parameterName)
        {
            return AddCondition(AnimatorConditionMode.IfNot, 0, parameterName);
        }

        public AnimatorStateTransitionBuilder Equals(AnimatorParameter param, float value)
        {
            return Equals(param.Name, value);
        }

        public AnimatorStateTransitionBuilder Equals(string parameterName, float value)
        {
            return AddCondition(AnimatorConditionMode.Equals, value, parameterName);
        }

        public AnimatorStateTransitionBuilder NotEquals(AnimatorParameter param, float value)
        {
            return NotEquals(param.Name, value);
        }

        public AnimatorStateTransitionBuilder NotEquals(string parameterName, float value)
        {
            return AddCondition(AnimatorConditionMode.NotEqual, value, parameterName);
        }

        public AnimatorStateTransitionBuilder Greater(AnimatorParameter param, float value)
        {
            return Greater(param.Name, value);
        }

        public AnimatorStateTransitionBuilder Greater(string parameterName, float value)
        {
            return AddCondition(AnimatorConditionMode.Greater, value, parameterName);
        }

        public AnimatorStateTransitionBuilder Less(AnimatorParameter param, float value)
        {
            return Less(param.Name, value);
        }

        public AnimatorStateTransitionBuilder Less(string parameterName, float value)
        {
            return AddCondition(AnimatorConditionMode.Less, value, parameterName);
        }

        public AnimatorStateTransition Build()
        {
            return _stateTransition;
        }
    }
}
