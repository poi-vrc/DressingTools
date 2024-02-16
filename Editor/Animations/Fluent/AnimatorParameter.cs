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

using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimatorParameter
    {
        public AnimatorControllerParameterType Type { get; set; }
        public string Name { get; set; }
        // public float DefaultValue { get; set; }

        public AnimatorParameter(AnimatorControllerParameterType type, string name)
        {
            Type = type;
            Name = name;
            // DefaultValue = defaultValue;
        }
    }
}
