/*
 * File: BlendshapeSyncModule.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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

using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    public class BlendshapeSyncModule : DTWearableModuleBase
    {
        public static class MessageCode
        {
        }

        private const string LogLabel = "BlendshapeSyncModule";

        public override int ApplyOrder => 6;

        public override bool AllowMultiple => false;

        public List<DTAnimationBlendshapeSync> blendshapeSyncs; // blendshapes to sync from avatar to wearables

        public BlendshapeSyncModule()
        {
            blendshapeSyncs = new List<DTAnimationBlendshapeSync>();
        }

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
            return true;
        }
    }
}
