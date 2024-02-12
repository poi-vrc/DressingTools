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

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    /// <summary>
    /// Animator builder to assist modifying an animator controller
    /// </summary>
    internal class AnimatorBuilder
    {
        private readonly AnimatorController _controller;
        private readonly AnimatorOptions _options;

        public AnimatorBuilder(AnimatorOptions options, AnimatorController controller)
        {
            _controller = controller;
            _options = options;
        }

        private int FindParameter(string name)
        {
            for (int i = 0; i < _controller.parameters.Length; i++)
            {
                if (name == _controller.parameters[i].name)
                {
                    return i;
                }
            }
            return -1;
        }

        public AnimatorParameter IntParameter(string name, int defaultValue = 0, bool forced = false)
        {
            return Parameter(AnimatorControllerParameterType.Int, name, defaultValue, forced);
        }

        public AnimatorParameter FloatParameter(string name, float defaultValue = 0.0f, bool forced = false)
        {
            return Parameter(AnimatorControllerParameterType.Float, name, defaultValue, forced);
        }

        public AnimatorParameter BoolParameter(string name, bool defaultValue = false, bool forced = false)
        {
            return Parameter(AnimatorControllerParameterType.Bool, name, defaultValue, forced);
        }

        private AnimatorParameter Parameter(AnimatorControllerParameterType type, string name, object defaultValue, bool forced)
        {
            var idx = FindParameter(name);
            if (idx != -1 && forced)
            {
                _controller.RemoveParameter(idx);
            }
            else if (idx != -1)
            {
                var existingType = _controller.parameters[idx].type;
                if (existingType != type)
                {
                    throw new System.Exception($"Parameter type mismatch, expected: {type} actual: {existingType}");
                }
            }
            else
            {
                var param = new AnimatorControllerParameter()
                {
                    name = name,
                    type = type
                };

                if (type == AnimatorControllerParameterType.Float)
                {
                    param.defaultFloat = (float)defaultValue;
                }
                else if (type == AnimatorControllerParameterType.Int)
                {
                    param.defaultInt = (int)defaultValue;
                }
                else if (type == AnimatorControllerParameterType.Bool)
                {
                    param.defaultBool = (bool)defaultValue;
                }

                _controller.AddParameter(param);
            }

            return new AnimatorParameter(type, name);
        }

        public AnimatorLayerBuilder NewLayer(string layerName)
        {
            var stateMachine = new AnimatorStateMachine()
            {
                name = layerName,
                hideFlags = HideFlags.HideInHierarchy
            };

            AssetDatabase.AddObjectToAsset(stateMachine, _controller);

            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                stateMachine = stateMachine,
                defaultWeight = 1.0f,
            };
            _controller.AddLayer(layer);

            return new AnimatorLayerBuilder(_options, layer);
        }

        public AnimatorController Build()
        {
            return _controller;
        }
    }
}
