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
using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimatorLayerBuilder
    {
        private readonly AnimatorControllerLayer _layer;
        private readonly AnimatorOptions _options;

        public AnimatorLayerBuilder(AnimatorOptions options, AnimatorControllerLayer layer)
        {
            _options = options;
            _layer = layer;
        }

        public AnimatorStateBuilder NewState(string name)
        {
            var state = _layer.stateMachine.AddState(name);
            state.writeDefaultValues = _options.writeDefaults;
            return new AnimatorStateBuilder(_options, state);
        }

        public AnimatorStateBuilder NewState(string name, Vector3 pos)
        {
            var state = _layer.stateMachine.AddState(name, pos);
            state.writeDefaultValues = _options.writeDefaults;
            return new AnimatorStateBuilder(_options, state);
        }

        public AnimatorLayerBuilder WithDefaultState(AnimatorStateBuilder state)
        {
            _layer.stateMachine.defaultState = state.Build();
            return this;
        }

        public AnimatorControllerLayer Build()
        {
            return _layer;
        }
    }
}
