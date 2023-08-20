/*
 * File: AnimationGenerationModule.cs
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
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class AnimationGenerationModule : WearableModuleBase
    {
        public static class MessageCode
        {
        }

        private const string LogLabel = "AnimationGenerationModule";

        public override int ApplyOrder => 4;

        public override bool AllowMultiple => false;

        public AnimationPreset avatarAnimationOnWear; // execute on wear

        public AnimationPreset wearableAnimationOnWear;

        public List<WearableCustomizable> wearableCustomizables; // items that show up in action menu for customization

        public AnimationGenerationModule()
        {
            avatarAnimationOnWear = new AnimationPreset();
            wearableAnimationOnWear = new AnimationPreset();
            wearableCustomizables = new List<WearableCustomizable>();
        }

        public override bool Apply(DTReport report, ICabinet cabinet, List<IDynamicsProxy> avatarDynamics, WearableConfig config, GameObject wearableGameObject)
        {
            return true;
        }
    }
}
