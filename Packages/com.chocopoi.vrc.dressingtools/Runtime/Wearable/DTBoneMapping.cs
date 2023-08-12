/*
 * File: DTBoneMapping.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
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

using System;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
{
    [Serializable]
    public enum DTBoneMappingType
    {
        DoNothing = 0,
        MoveToBone = 1,
        ParentConstraint = 2,
        IgnoreTransform = 3,
        CopyDynamics = 4
    }

    [Serializable]
    public class DTBoneMapping
    {
        public DTBoneMappingType mappingType;
        public string avatarBonePath;
        public string wearableBonePath;

        public bool Equals(DTBoneMapping x)
        {
            return mappingType == x.mappingType && avatarBonePath == x.avatarBonePath && wearableBonePath == x.wearableBonePath;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} -> {2}", mappingType, wearableBonePath, avatarBonePath);
        }
    }
}
