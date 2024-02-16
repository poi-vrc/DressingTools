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
    /// Bone mapping
    /// </summary>
    [Serializable]
    internal class BoneMapping
    {
        /// <summary>
        /// Mapping type
        /// </summary>
        public BoneMappingType mappingType;

        /// <summary>
        /// Bone path relative to avatar root
        /// </summary>
        public string avatarBonePath;

        /// <summary>
        /// Bone path relative to wearable root
        /// </summary>
        public string wearableBonePath;

        /// <summary>
        /// Check if equals to another mapping
        /// </summary>
        /// <param name="mapping">Another bone mapping</param>
        /// <returns></returns>
        public bool Equals(BoneMapping mapping)
        {
            return mappingType == mapping.mappingType && avatarBonePath == mapping.avatarBonePath && wearableBonePath == mapping.wearableBonePath;
        }

        /// <summary>
        /// Returns a string representable form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1} -> {2}", mappingType, wearableBonePath, avatarBonePath);
        }
    }
}
