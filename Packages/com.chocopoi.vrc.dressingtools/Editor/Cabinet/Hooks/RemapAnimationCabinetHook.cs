/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingFramework.
 * 
 * DressingFramework is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingFramework is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Proxy;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet.Hooks
{
    internal class RemapAnimationCabinetHook : CabinetHookBase
    {
        public override string FriendlyName => "Remap Animations";

        public override CabinetApplyConstraint Constraint =>
            ApplyAtStage(CabinetApplyStage.Transpose, CabinetHookStageRunOrder.After)
                .Build();

        public override bool Invoke(ApplyCabinetContext cabCtx)
        {
            var dkCabCtx = cabCtx.Extra<DKCabinetContext>();

            foreach (var clipContainer in dkCabCtx.animationStore.Clips)
            {
                var oldClip = clipContainer.originalClip;
                var remapped = false;

                var newClip = new AnimationClip()
                {
                    name = oldClip.name,
                    legacy = oldClip.legacy,
                    frameRate = oldClip.frameRate,
                    localBounds = oldClip.localBounds,
                    wrapMode = oldClip.wrapMode
                };
                AnimationUtility.SetAnimationClipSettings(newClip, AnimationUtility.GetAnimationClipSettings(oldClip));

                var curveBindings = AnimationUtility.GetCurveBindings(oldClip);
                foreach (var curveBinding in curveBindings)
                {
                    var avoidContainerBones = curveBinding.type == typeof(Transform);
                    var newPath = dkCabCtx.pathRemapper.Remap(curveBinding.path, avoidContainerBones);

                    remapped |= newPath != curveBinding.path;

                    newClip.SetCurve(newPath, curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(oldClip, curveBinding));
                }

                var objRefBindings = AnimationUtility.GetObjectReferenceCurveBindings(oldClip);
                foreach (var objRefBinding in objRefBindings)
                {
                    var newPath = dkCabCtx.pathRemapper.Remap(objRefBinding.path, false);

                    remapped |= newPath != objRefBinding.path;

                    var newObjRefBinding = objRefBinding;
                    newObjRefBinding.path = newPath;
                    AnimationUtility.SetObjectReferenceCurve(newClip, newObjRefBinding, AnimationUtility.GetObjectReferenceCurve(oldClip, objRefBinding));
                }

                if (remapped)
                {
                    clipContainer.newClip = newClip;
                }
            }
            return true;
        }
    }
}
