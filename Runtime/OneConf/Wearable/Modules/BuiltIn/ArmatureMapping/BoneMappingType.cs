/*
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

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping
{
    /// <summary>
    /// Bone mapping type
    /// </summary>
    [Serializable]
    internal enum BoneMappingType
    {
        /// <summary>
        /// Do nothing
        /// </summary>
        DoNothing = 0,

        /// <summary>
        /// Move wearable bone to avatar bone
        /// </summary>
        MoveToBone = 1,

        /// <summary>
        /// Bind using ParentConstraint
        /// </summary>
        ParentConstraint = 2,

        /// <summary>
        /// Add IgnoreTransform recursively downwards on dynamics
        /// </summary>
        IgnoreTransform = 3,

        /// <summary>
        /// Copy avatar dynamics data to wearable
        /// </summary>
        CopyDynamics = 4
    }
}
