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
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet.Hooks
{
    internal class GroupDynamicsWearableHook : WearableHookBase
    {
        private const string DynamicsContainerName = "DT_Dynamics";

        public override string Identifier => "com.chocopoi.vrc.dressingtools.hooks.wearable.group-dynamics";
        public override string FriendlyName => "Group Dynamics";

        public override WearableApplyConstraint Constraint => ApplyAtStage(CabinetApplyStage.Optimization).Build();

        private void CopyDynamicsToContainer(IDynamicsProxy dynamics, GameObject dynamicsContainer)
        {
            // in our PhysBoneProxy, we return the current transform if rootTransform is null
            // so if we move it away, the controlling transform will be incorrect. so we are
            // setting the transform again here.
            if (dynamics is PhysBoneProxy && dynamics.RootTransform == dynamics.Transform)
            {
                dynamics.RootTransform = dynamics.Transform;
            }

            // copy to dynamics container
            var newComp = DTEditorUtils.CopyComponent(dynamics.Component, dynamicsContainer);

            // destroy the original one
            Object.DestroyImmediate(dynamics.Component);

            // set the component in our proxy to the new component
            dynamics.Component = newComp;
        }

        public override bool Invoke(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx)
        {
            if (!cabCtx.cabinetConfig.groupDynamics) return true;

            var dkWearCtx = wearCtx.Extra<DKWearableContext>();

            // no need to group if no dynamics
            if (dkWearCtx.wearableDynamics.Count == 0) return true;

            // create dynamics container (reuse if originally have)
            var dynamicsContainer = wearCtx.wearableGameObject.transform.Find(DynamicsContainerName);
            if (dynamicsContainer == null)
            {
                var obj = new GameObject(DynamicsContainerName);
                obj.transform.SetParent(wearCtx.wearableGameObject.transform);
                dynamicsContainer = obj.transform;
            }

            if (cabCtx.cabinetConfig.groupDynamicsSeparateGameObjects)
            {
                // group them in separate GameObjects
                var addedNames = new Dictionary<string, int>();
                foreach (var dynamics in dkWearCtx.wearableDynamics)
                {
                    var name = dynamics.RootTransform.name;

                    // we might occur cases with dynamics' bone name are the same
                    if (!addedNames.TryGetValue(name, out int count))
                    {
                        count = 0;
                    }

                    // we don't add suffix for the first occurance
                    var containerName = count == 0 ? name : string.Format("{0}_{1}", name, count);
                    var container = new GameObject(containerName);
                    container.transform.SetParent(dynamicsContainer);

                    CopyDynamicsToContainer(dynamics, container);

                    addedNames[name] = ++count;
                }
            }
            else
            {
                // we just group them into a single GameObject
                foreach (var dynamics in dkWearCtx.wearableDynamics)
                {
                    CopyDynamicsToContainer(dynamics, dynamicsContainer.gameObject);
                }
            }

            return true;
        }
    }
}
