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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTIgnoreDynamics))]
    internal class IgnoreDynamicsPass : ComponentPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .AfterPass<CopyDynamicsPass>()
                .Build();

        private static IDynamics FindDynamics(List<IDynamics> allDynamics, Transform root)
        {
            return allDynamics
                .Where(d =>
                    !(d is VRMSpringBoneProxy) &&
                    d.RootTransforms.Contains(root))
                .FirstOrDefault();
        }

        private static void LookUpAndIgnore(Context ctx, List<IDynamics> allDynamics, DTIgnoreDynamics comp)
        {
            var p = comp.transform.parent;
            while (p != null)
            {
                var dynamics = FindDynamics(allDynamics, p);
                if (dynamics != null)
                {
                    dynamics.IgnoreTransforms.Add(comp.transform);
                    return;
                }
                p = p.parent;
            }
        }

        private bool InvokeSingleComponentWithDynamicsList(Context ctx, List<IDynamics> allDynamics, DTIgnoreDynamics comp)
        {
            LookUpAndIgnore(ctx, allDynamics, comp);
            return true;
        }

        public override bool Invoke(Context ctx)
        {
            var allDynamics = DynamicsUtils.ScanDynamics(ctx.AvatarGameObject);
            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTIgnoreDynamics>(true);
            foreach (var comp in comps)
            {
                LookUpAndIgnore(ctx, allDynamics, comp);
            }
            return true;
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var allDynamics = DynamicsUtils.ScanDynamics(ctx.AvatarGameObject);
            var comp = (DTIgnoreDynamics)component;
            LookUpAndIgnore(ctx, allDynamics, comp);
            return true;
        }
    }
}
