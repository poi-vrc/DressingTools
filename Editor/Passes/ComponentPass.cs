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
using System.Reflection;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components;

namespace Chocopoi.DressingTools.Passes
{
    internal abstract class ComponentPass : BuildPass
    {
        public Type ComponentType { get => GetType().GetCustomAttribute<ComponentPassFor>()?.Type; }

        public override bool Invoke(Context ctx)
        {
            if (!ComponentType.IsSubclassOf(typeof(DTBaseComponent)))
            {
                ctx.Report.LogError("ComponentPass", $"{ComponentType.FullName} is not a subclass of DTBaseComponent");
                return false;
            }
            var comps = ctx.AvatarGameObject.GetComponentsInChildren(ComponentType, true);
            foreach (var comp in comps)
            {
                if (!Invoke(ctx, (DTBaseComponent)comp, out _))
                {
                    return false;
                }
            }
            return true;
        }

        public abstract bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents);
    }
}
