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

using System.Collections.Generic;
using System.Linq;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Detail.DK.Passes;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    internal class GroupDynamicsModifyAnimPass : BuildPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .AfterPass<GroupDynamicsPass>()
                .BeforePass<DispatchAnimationStorePass>()
                .Build();

        private static void ModifyAnimations(Context ctx, Dictionary<DTGroupDynamics, List<IDynamics>> groups)
        {
            var store = ctx.Feature<AnimationStore>();
            foreach (var clipContainer in store.Clips)
            {
                var oldClip = clipContainer.newClip == null ? clipContainer.originalClip : clipContainer.newClip;
                var newClip = DTEditorUtils.CopyClip(oldClip);
                var modified = false;

                var bindings = AnimationUtility.GetCurveBindings(oldClip).Where(b => b.propertyName == "m_Enabled" && b.type == typeof(DTGroupDynamics)).ToList();
                if (bindings.Count == 0)
                {
                    continue;
                }

                foreach (var oldBinding in bindings)
                {
                    var compTransform = ctx.AvatarGameObject.transform.Find(oldBinding.path);
                    if (compTransform == null || !compTransform.TryGetComponent<DTGroupDynamics>(out var comp))
                    {
                        Debug.LogWarning("[DressingTools] An animation contains binding to DTGroupDynamics but it cannot be found, ignoring: " + oldBinding.path);
                        modified = true;
                        AnimationUtility.SetEditorCurve(newClip, oldBinding, null);
                        continue;
                    }

                    if (!groups.ContainsKey(comp))
                    {
                        continue;
                    }

                    // replace with our dynamics components
                    var curve = AnimationUtility.GetEditorCurve(oldClip, oldBinding);
                    var list = groups[comp];
                    foreach (var dynamics in list)
                    {
                        var newBinding = new EditorCurveBinding()
                        {
                            type = dynamics.Component.GetType(),
                            propertyName = "m_Enabled",
                            path = AnimationUtils.GetRelativePath(dynamics.Transform, ctx.AvatarGameObject.transform)
                        };
                        AnimationUtility.SetEditorCurve(newClip, newBinding, curve);
                    }

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

        public override bool Invoke(Context ctx)
        {
            var dtCtx = ctx.Extra<DressingToolsContext>();
            ModifyAnimations(ctx, dtCtx.DynamicsGroups);
            return true;
        }
    }
}
