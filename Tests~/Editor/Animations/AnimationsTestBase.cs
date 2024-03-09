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
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Animations.Fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Animations
{
    internal abstract class AnimationsTestBase : EditorTestBase
    {
        public void SetupEnv(out GameObject root, out AnimatorOptions options, out AnimatorController ac)
        {
            root = CreateGameObject("Avatar");
            var ctx = new DKNativeContext(root);
            options = new AnimatorOptions()
                .WithRootTransform(root.transform)
                .WithWriteDefaults(true)
                .WithContext(ctx);
            ac = new AnimatorController();
            ctx.CreateUniqueAsset(ac, "TestAnimatorController");
        }

        public bool CurvesAreEqual(AnimationCurve a, AnimationCurve b)
        {
            if (a.length != b.length)
            {
                return false;
            }

            for (var i = 0; i < a.length; i++)
            {
                if (!a.keys[i].Equals(b.keys[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasEditorCurve(AnimationClip clip, string path, Type type, string property, AnimationCurve expectedCurve)
        {
            var binding = new EditorCurveBinding()
            {
                path = path,
                propertyName = property,
                type = type
            };

            var actualCurve = AnimationUtility.GetEditorCurve(clip, binding);
            if (actualCurve == null)
            {
                Debug.Log($"Not found curve {path}: {type.FullName} {property}");
                return false;
            }

            var result = CurvesAreEqual(actualCurve, expectedCurve);
            if (!result)
            {
                Debug.Log($"Actual curve does not match with expected!");
            }

            return result;
        }

        public bool HasObjRefCurve(AnimationClip clip, string path, Type type, string property, ObjectReferenceKeyframe[] expected)
        {
            var binding = new EditorCurveBinding()
            {
                path = path,
                propertyName = property,
                type = type
            };

            var actual = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (expected.Length != actual.Length)
            {
                return false;
            }

            for (var i = 0; i < expected.Length; i++)
            {
                if (expected[i].time != actual[i].time ||
                    expected[i].value != actual[i].value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
