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
using System.Collections.Generic;
using System.Linq;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTCopyDynamics))]
    internal class CopyDynamicsPass : ComponentPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<GroupDynamicsPass>()
                .Build();

        private static Dictionary<DTCopyDynamics, Component> MakeCopies(Context ctx, DTCopyDynamics[] copyDynComps)
        {
            var copies = new Dictionary<DTCopyDynamics, Component>();
            foreach (var copyDynComp in copyDynComps)
            {
                var originalDynamics = FindDynamics(ctx.AvatarGameObject.transform, copyDynComp);
                if (originalDynamics == null)
                {
                    ctx.Report.LogWarn("CopyDynamicsPass", $"Dynamics not found for copying at path, ignoring: {copyDynComp.SourcePath} in mode {copyDynComp.SourceSearchMode}");
                    continue;
                }

                // copy component with reflection
                var copiedDynamics = DKEditorUtils.CopyComponent(originalDynamics.Component, copyDynComp.gameObject);

                // set root transform
                if (originalDynamics is DynamicBoneProxy)
                {
                    new DynamicBoneProxy(copiedDynamics)
                    {
                        RootTransform = copyDynComp.transform
                    };
                }
                else if (originalDynamics is PhysBoneProxy)
                {
                    new PhysBoneProxy(copiedDynamics)
                    {
                        RootTransform = copyDynComp.transform
                    };
                }

                copies[copyDynComp] = copiedDynamics;
            }
            return copies;
        }

        private static void ModifyAnimations(Context ctx, Dictionary<DTCopyDynamics, Component> copies)
        {
            var store = ctx.Feature<AnimationStore>();
            foreach (var clipContainer in store.Clips)
            {
                var oldClip = clipContainer.newClip != null ? clipContainer.newClip : clipContainer.originalClip;
                var newClip = DTEditorUtils.CopyClip(oldClip);
                var modified = false;

                var bindings = AnimationUtility.GetCurveBindings(oldClip).Where(b => b.propertyName == "m_Enabled" && b.type == typeof(DTCopyDynamics)).ToList();
                if (bindings.Count == 0)
                {
                    continue;
                }

                foreach (var oldBinding in bindings)
                {
                    var compTransform = ctx.AvatarGameObject.transform.Find(oldBinding.path);
                    if (compTransform == null || !compTransform.TryGetComponent<DTCopyDynamics>(out var comp))
                    {
                        Debug.LogWarning("[DressingTools] An animation contains binding to DTCopyDynamics but it cannot be found, ignoring: " + oldBinding.path);
                        modified = true;
                        AnimationUtility.SetEditorCurve(newClip, oldBinding, null);
                        continue;
                    }

                    if (!copies.ContainsKey(comp))
                    {
                        continue;
                    }

                    // replace with our dynamics components
                    var curve = AnimationUtility.GetEditorCurve(oldClip, oldBinding);
                    var dynamicsComp = copies[comp];

                    var newBinding = new EditorCurveBinding()
                    {
                        type = dynamicsComp.GetType(),
                        propertyName = "m_Enabled",
                        path = AnimationUtils.GetRelativePath(dynamicsComp.transform, ctx.AvatarGameObject.transform)
                    };
                    AnimationUtility.SetEditorCurve(newClip, newBinding, curve);

                    // mark as modified and remove the group dynamics component curve
                    modified = true;
                    AnimationUtility.SetEditorCurve(newClip, oldBinding, null);
                }

                if (modified)
                {
                    clipContainer.newClip = newClip;
                }
            }
        }

        private static IDynamics FindDynamics(Transform root, DTCopyDynamics comp)
        {
            var targetRoot = string.IsNullOrEmpty(comp.SourcePath) ? root : root.Find(comp.SourcePath);

            var allDynamics = DynamicsUtils.ScanDynamics(root.gameObject);

            if (comp.SourceSearchMode == DTCopyDynamics.DynamicsSearchMode.ControlRoot)
            {
                return allDynamics.Where(d => d.RootTransforms.Contains(targetRoot)).FirstOrDefault();
            }
            else if (comp.SourceSearchMode == DTCopyDynamics.DynamicsSearchMode.ComponentRoot)
            {
                return allDynamics.Where(d => d.Transform == targetRoot).FirstOrDefault();
            }

            return null;
        }

        public override bool Invoke(Context ctx)
        {
            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTCopyDynamics>(true);
            var copies = MakeCopies(ctx, comps);
            ModifyAnimations(ctx, copies);
            return true;
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var copies = MakeCopies(ctx, new DTCopyDynamics[] { (DTCopyDynamics)component });
            ModifyAnimations(ctx, copies);
            return true;
        }
    }
}
