/*
 * File: DTWearableCustomizable.cs
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

using System;
using System.Collections.Generic;

namespace Chocopoi.DressingTools.Lib.Wearable
{
    [Serializable]
    public enum WearableCustomizableType
    {
        Toggle = 0,
        Blendshape = 1
    }

    [Serializable]
    public class WearableCustomizable
    {
        public WearableCustomizableType type;
        public List<AnimationToggle> avatarRequiredToggles;
        public List<AnimationToggle> wearableToggles;
        public List<AnimationBlendshapeValue> avatarRequiredBlendshapes;
        public List<AnimationBlendshapeValue> wearableBlendshapes;

        public WearableCustomizable()
        {
            type = WearableCustomizableType.Toggle;
            avatarRequiredToggles = new List<AnimationToggle>();
            wearableToggles = new List<AnimationToggle>();
            avatarRequiredBlendshapes = new List<AnimationBlendshapeValue>();
            wearableBlendshapes = new List<AnimationBlendshapeValue>();
        }
    }
}
