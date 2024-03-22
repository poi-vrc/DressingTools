/*
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

using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components.Modifiers;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Wearable.Passes
{
    internal class GroupDynamicsWearablePass : WearablePass
    {
        public const string DynamicsContainerName = "DT_Dynamics";

        public override string FriendlyName => "Group Dynamics";

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Generation)
                .Build();

        public override bool Invoke(CabinetContext cabCtx, WearableContext wearCtx)
        {
            if (!cabCtx.cabinetConfig.groupDynamics) return true;

            // no need to group if no dynamics
            if (wearCtx.wearableDynamics.Count == 0) return true;

            // create dynamics container (reuse if originally have)
            var dynamicsContainer = wearCtx.wearableGameObject.transform.Find(DynamicsContainerName);
            if (dynamicsContainer == null)
            {
                var obj = new GameObject(DynamicsContainerName);
                obj.transform.SetParent(wearCtx.wearableGameObject.transform);
                dynamicsContainer = obj.transform;
            }

            var comp = dynamicsContainer.gameObject.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = cabCtx.cabinetConfig.groupDynamicsSeparateGameObjects;
            comp.SetToCurrentState = false;
            comp.enabled = false;

            foreach (var dynamics in wearCtx.wearableDynamics)
            {
                foreach (var rootTransform in dynamics.RootTransforms)
                {
                    comp.IncludeTransforms.Add(rootTransform);
                }
            }

            return true;
        }
    }
}
