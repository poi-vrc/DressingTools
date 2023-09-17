/*
 * File: IAnimationStore.cs
 * Project: DressingTools
 * Created Date: Sunday, September 17th 2023, 4:31:58 pm
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
using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Animations
{
    public class AnimationClipContainer
    {
        public AnimationClip originalClip;
        public AnimationClip newClip;
        public Action<AnimationClip> dispatchFunc;

        public AnimationClipContainer()
        {
            originalClip = null;
            newClip = null;
            dispatchFunc = null;
        }
    }

    public interface IAnimationStore
    {
        event Action Write;
        List<AnimationClipContainer> Clips { get; }

        void Dispatch();
        void RegisterMotion(Motion motion, Action<AnimationClip> dispatchFunc, Func<Motion, bool> filterMotionFunc = null, HashSet<Motion> visitedMotions = null);
        void RegisterClip(AnimationClip clip, Action<AnimationClip> dispatchFunc);
    }
}
