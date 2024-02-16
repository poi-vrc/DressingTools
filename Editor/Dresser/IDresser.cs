/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;

namespace Chocopoi.DressingTools.Dresser
{
    /// <summary>
    /// Dresser interface
    /// </summary>
    internal interface IDresser
    {
        /// <summary>
        /// Human-readable friendly name of this dresser
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Deserialize settings JSON (This API will be changed soon)
        /// </summary>
        /// <param name="serializedJson">Serialized JSON</param>
        /// <returns>Abstracted dresser settings</returns>
        DresserSettings DeserializeSettings(string serializedJson);

        /// <summary>
        /// Create new dresser settings
        /// </summary>
        /// <returns>New dresser settings</returns>
        DresserSettings NewSettings();

        /// <summary>
        /// Execute with provided settings and output bone mappings
        /// </summary>
        /// <param name="settings">Dresser settings, must be compatible with the dresser</param>
        /// <param name="boneMappings">Output bone mappings, `null` if error or no mappings can be generated</param>
        /// <returns>Report during bone mapping generation</returns>
        DKReport Execute(DresserSettings settings, out List<BoneMapping> boneMappings);
    }
}
