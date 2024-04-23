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

namespace Chocopoi.DressingTools.Dresser.Standard
{
    internal class StandardDresserSettings : DresserSettingsBase
    {
        public enum DynamicsOptions
        {
            Auto = 0,
            RemoveDynamicsAndUseParentConstraint = 1,
            KeepDynamicsAndUseParentConstraintIfNecessary = 2,
            IgnoreTransform = 3,
            CopyDynamics = 4,
            IgnoreAll = 5,
        }

        public DynamicsOptions DynamicsOption { get; set; }
    }
}
