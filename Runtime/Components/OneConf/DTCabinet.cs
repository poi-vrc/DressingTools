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

using Chocopoi.DressingTools.OneConf.Cabinet;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.OneConf
{
    /// <summary>
    /// DressingTools cabinet component. Used for storing cabinet settings and apply trigger.
    /// </summary>
    [AddComponentMenu("DressingTools/DT Cabinet")]
    [DefaultExecutionOrder(-19999)]
    [ExecuteInEditMode]
    internal class DTCabinet : DTBaseComponent
    {
        /// <summary>
        /// Root GameObject. Set to `null` to use the GameObject holding this component.
        /// </summary>
        public GameObject rootGameObject;

        /// <summary>
        /// Config JSON. This is the same as the property `ConfigJson`.
        /// </summary>
        public string configJson;

        public DTCabinet()
        {
            rootGameObject = null;
            configJson = JsonConvert.SerializeObject(new CabinetConfig());
        }
    }
}
