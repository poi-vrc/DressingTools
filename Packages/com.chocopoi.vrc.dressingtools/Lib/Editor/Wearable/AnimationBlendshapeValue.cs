/*
 * File: DTAnimationBlendshapeValue.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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

namespace Chocopoi.DressingTools.Lib.Wearable
{
    [Serializable]
    public class AnimationBlendshapeValue
    {
        public string path;
        public string blendshapeName;
        public float value;

        public AnimationBlendshapeValue()
        {
            path = null;
            blendshapeName = null;
            value = 0;
        }

        public AnimationBlendshapeValue(AnimationBlendshapeValue animationBlendshapeValue)
        {
            path = animationBlendshapeValue.path;
            blendshapeName = animationBlendshapeValue.blendshapeName;
            value = animationBlendshapeValue.value;
        }
    }
}
