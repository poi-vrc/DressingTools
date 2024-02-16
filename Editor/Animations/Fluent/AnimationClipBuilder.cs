/*
 * Copyright (c) 2024 chocopoi
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
using Chocopoi.DressingFramework.Animations;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimationClipBuilder
    {
        private readonly AnimatorOptions _options;
        private readonly AnimationClip _clip;

        public AnimationClipBuilder(AnimatorOptions options, AnimationClip clip)
        {
            _options = options;
            _clip = clip;
        }

        private string GetRelativePath(Transform root, Transform trans)
        {
            string path = trans.name;
            while (true)
            {
                trans = trans.parent;

                if (trans.parent == null || root == trans)
                {
                    break;
                }

                path = trans.name + "/" + path;
            }
            var pm = _options.context.Feature<PathRemapper>();
            return pm.Remap(path);
        }

        public AnimationClipBuilder SetCurve(string relativePath, Type type, string propertyName, AnimationCurve curve)
        {
            _clip.SetCurve(relativePath, type, propertyName, curve);
            return this;
        }

        public AnimationClipBuilder SetCurve(Transform transform, Type type, string propertyName, AnimationCurve curve)
        {
            _clip.SetCurve(GetRelativePath(_options.rootTransform, transform), type, propertyName, curve);
            return this;
        }

        public AnimationClipBuilder SetCurve(Transform transform, Type type, string propertyName, ObjectReferenceKeyframe[] frames)
        {
            var binding = new EditorCurveBinding()
            {
                path = GetRelativePath(_options.rootTransform, transform),
                type = type,
                propertyName = propertyName
            };
            AnimationUtility.SetObjectReferenceCurve(_clip, binding, frames);
            return this;
        }

        public AnimationClipBuilder Toggle(Component comp, bool enabled)
        {
            _clip.SetCurve(GetRelativePath(_options.rootTransform, comp.transform), comp.GetType(), "m_Enabled", AnimationCurve.Constant(0.0f, 0.0f, enabled ? 1.0f : 0.0f));
            return this;
        }

        public AnimationClipBuilder Toggle(GameObject go, bool enabled)
        {
            _clip.SetCurve(GetRelativePath(_options.rootTransform, go.transform), typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0.0f, 0.0f, enabled ? 1.0f : 0.0f));
            return this;
        }

        public AnimationClipBuilder Blendshape(SkinnedMeshRenderer smr, string blendshapeName, float value)
        {
            return Blendshape(smr, blendshapeName, AnimationCurve.Constant(0.0f, 0.0f, value));
        }

        public AnimationClipBuilder Blendshape(SkinnedMeshRenderer smr, string blendshapeName, AnimationCurve curve)
        {
            _clip.SetCurve(GetRelativePath(_options.rootTransform, smr.transform), typeof(SkinnedMeshRenderer), "blendShape." + blendshapeName, curve);
            return this;
        }

        public AnimationClip Build()
        {
            return _clip;
        }
    }
}
