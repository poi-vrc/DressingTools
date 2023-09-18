/*
 * File: AnimationStore.cs
 * Project: DressingTools
 * Created Date: Sunday, September 17th 2023, 3:43:20 pm
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
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Animations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class AnimationStore : IAnimationStore
    {
        public event Action Write;

        public List<AnimationClipContainer> Clips { get; private set; }

        private ApplyCabinetContext _cabCtx;

        public AnimationStore(ApplyCabinetContext cabCtx)
        {
            _cabCtx = cabCtx;
            Clips = new List<AnimationClipContainer>();
        }

        public void Dispatch()
        {
            foreach (var clip in Clips)
            {
                if (clip.newClip != null && clip.originalClip != clip.newClip)
                {
                    _cabCtx.CreateUniqueAsset(clip.newClip, $"Clip_{clip.newClip.name}_{DTEditorUtils.RandomString(6)}.anim");
                    clip.dispatchFunc?.Invoke(clip.newClip);
                }
            }
            Write?.Invoke();
        }

        public void RegisterMotion(Motion motion, Action<AnimationClip> dispatchFunc, Func<Motion, bool> filterMotionFunc = null, HashSet<Motion> visitedMotions = null)
        {
            if (visitedMotions == null)
            {
                visitedMotions = new HashSet<Motion>();
            }

            if (motion == null || (filterMotionFunc != null && !filterMotionFunc(motion)))
            {
                return;
            }

            // avoid visited motions
            if (visitedMotions.Contains(motion))
            {
                return;
            }
            visitedMotions.Add(motion);

            if (motion is AnimationClip clip)
            {
                RegisterClip(clip, dispatchFunc);
            }
            else if (motion is BlendTree tree)
            {
                for (var i = 0; i < tree.children.Length; i++)
                {
                    var childIndex = i;
                    RegisterMotion(motion, (AnimationClip newClip) => tree.children[childIndex].motion = newClip, filterMotionFunc, visitedMotions);
                }
            }
        }

        public void RegisterClip(AnimationClip clip, Action<AnimationClip> dispatchFunc)
        {
            Clips.Add(new AnimationClipContainer()
            {
                originalClip = clip,
                dispatchFunc = dispatchFunc
            });
        }
    }
}
