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

using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn
{
    /// <summary>
    /// Move root wearable module config 
    /// </summary>
    internal class MoveRootWearableModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.built-in.wearable.move-root";

        /// <summary>
        /// Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        ///  Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Avatar path to move to
        /// </summary>
        public string avatarPath;

        /// <summary>
        /// Constructs a new move root wearable module config
        /// </summary>
        public MoveRootWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            avatarPath = null;
        }
    }
}
