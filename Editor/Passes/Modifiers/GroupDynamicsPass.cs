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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTGroupDynamics))]
    internal class GroupDynamicsPass : ComponentPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<GroupDynamicsModifyAnimPass>()
                .Build();

        private static bool IsIncluded(ICollection<Transform> rootTransforms, List<Transform> includedTransforms, List<Transform> excludedTransforms)
        {
            return rootTransforms.All(t => IsIncluded(t, includedTransforms, excludedTransforms));
        }

        private static bool IsIncluded(Transform rootTransform, List<Transform> includedTransforms, List<Transform> excludedTransforms)
        {
            if (includedTransforms.Contains(rootTransform) &&
                !excludedTransforms.Contains(rootTransform))
            {
                return true;
            }

            if (excludedTransforms.Contains(rootTransform))
            {
                return false;
            }

            if (!includedTransforms.Any(t => DKEditorUtils.IsGrandParent(t, rootTransform)))
            {
                return false;
            }

            if (excludedTransforms.Any(t => DKEditorUtils.IsGrandParent(t, rootTransform)))
            {
                return false;
            }

            return true;
        }

        private static List<IDynamics> GetGroup(List<IDynamics> allDynamics, DTGroupDynamics comp)
        {
            if (comp.SearchMode == DTGroupDynamics.DynamicsSearchMode.ControlRoot)
            {
                return allDynamics.Where(d => IsIncluded(d.RootTransforms, comp.IncludeTransforms, comp.ExcludeTransforms)).ToList();
            }
            else if (comp.SearchMode == DTGroupDynamics.DynamicsSearchMode.ComponentRoot)
            {
                return allDynamics.Where(d => IsIncluded(d.Transform, comp.IncludeTransforms, comp.ExcludeTransforms)).ToList();
            }
            else
            {
                return new List<IDynamics>();
            }
        }

        private static Dictionary<DTGroupDynamics, List<IDynamics>> GetGroups(GameObject avatarGameObject)
        {
            var groupedDynamics = new Dictionary<DTGroupDynamics, List<IDynamics>>();

            // find all avatar dynamics first
            var allDynamics = DynamicsUtils.ScanDynamics(avatarGameObject);
            var comps = avatarGameObject.GetComponentsInChildren<DTGroupDynamics>(true);

            foreach (var comp in comps)
            {
                var list = GetGroup(allDynamics, comp);
                groupedDynamics[comp] = list;
                list.ForEach(d => allDynamics.Remove(d));
            }

            return groupedDynamics;
        }

        private static void CopyDynamicsToContainer(IDynamics dynamics, GameObject dynamicsContainer)
        {
            // in our PhysBoneProxy, we return the current transform if rootTransform is null
            // so if we move it away, the controlling transform will be incorrect. so we are
            // setting the transform again here.
            if (dynamics is PhysBoneProxy physBoneProxy && physBoneProxy.RootTransform == dynamics.Transform)
            {
                physBoneProxy.RootTransform = dynamics.Transform;
            }

            // copy to dynamics container
            var newComp = DKEditorUtils.CopyComponent(dynamics.Component, dynamicsContainer);

            // destroy the original one
            Object.DestroyImmediate(dynamics.Component);

            // set the component in our proxy to the new component
            dynamics.Component = newComp;
        }

        private static void GroupDynamics(DTGroupDynamics comp, List<IDynamics> list)
        {
            // set them to the current state of the component
            if (comp.SetToCurrentState)
            {
                foreach (var dynamics in list)
                {
                    if (dynamics.Component is Behaviour b)
                    {
                        b.enabled = comp.enabled;
                    }
                }
            }

            if (comp.SeparateGameObjects)
            {
                // group them in separate GameObjects
                var addedNames = new Dictionary<string, int>();
                foreach (var dynamics in list)
                {
                    var firstRootTransform = dynamics.RootTransforms.FirstOrDefault();
                    if (firstRootTransform == null)
                    {
                        // use the current transform instead if null
                        firstRootTransform = dynamics.Transform;
                    }
                    var name = firstRootTransform.name;

                    // we might occur cases with dynamics' bone name are the same
                    if (!addedNames.TryGetValue(name, out int count))
                    {
                        count = 0;
                    }

                    // we don't add suffix for the first occurance
                    var containerName = count == 0 ? name : string.Format("{0}_{1}", name, count);
                    var container = new GameObject(containerName);
                    container.transform.SetParent(comp.transform);

                    CopyDynamicsToContainer(dynamics, container);

                    addedNames[name] = ++count;
                }
            }
            else
            {
                // we just group them into a single GameObject
                foreach (var dynamics in list)
                {
                    CopyDynamicsToContainer(dynamics, comp.gameObject);
                }
            }
        }

        public override bool Invoke(Context ctx)
        {
            var dtCtx = ctx.Extra<DressingToolsContext>();
            dtCtx.DynamicsGroups.Clear();

            var groups = GetGroups(ctx.AvatarGameObject);
            if (groups.Count == 0)
            {
                return true;
            }

            foreach (var group in groups)
            {
                dtCtx.DynamicsGroups[group.Key] = group.Value;
                GroupDynamics(group.Key, group.Value);
            }

            return true;
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var comp = (DTGroupDynamics)component;

            var allDynamics = DynamicsUtils.ScanDynamics(ctx.AvatarGameObject);
            var list = GetGroup(allDynamics, comp);
            GroupDynamics(comp, list);

            // TODO: Modify animations for component apply
            // ModifyAnimations(ctx, groups);

            return true;
        }
    }
}
