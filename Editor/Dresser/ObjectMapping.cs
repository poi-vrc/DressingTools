/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using UnityEngine;

namespace Chocopoi.DressingTools.Dresser
{
    internal class ObjectMapping
    {
        public enum MappingType
        {
            MoveToBone = 0,
            ParentConstraint = 1,
            IgnoreTransform = 2,
        }

        public MappingType Type { get; set; }
        public Transform SourceTransform { get; set; }
        public string TargetPath { get; set; }

        public bool Equals(ObjectMapping mapping)
        {
            return Type == mapping.Type && SourceTransform == mapping.SourceTransform && TargetPath == mapping.TargetPath;
        }

        public override string ToString()
        {
            return $"{Type}: {SourceTransform.name} -> {TargetPath}";
        }
    }
}
