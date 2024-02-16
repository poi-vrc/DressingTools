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

#if DT_VRCSDK3A
using Chocopoi.DressingTools.OneConf.Serialization;

namespace Chocopoi.DressingTools.OneConf.Cabinet.Modules.Integrations.VRChat
{
    /// <summary>
    /// VRChat cabinet animation module config
    /// </summary>
    internal class VRCCabinetAnimCabinetModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.integrations.vrchat.cabinet.cabinet-anim";

        /// <summary>
        /// Enable cabinet thumbnails to use in toggle icons
        /// </summary>
        public bool cabinetThumbnails;

        /// <summary>
        /// Constructs new VRChat cabinet animation module config
        /// </summary>
        public VRCCabinetAnimCabinetModuleConfig()
        {
            cabinetThumbnails = true;
        }
    }
}
#endif
