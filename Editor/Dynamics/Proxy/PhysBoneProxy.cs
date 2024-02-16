/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.DressingFramework;
using UnityEngine;

namespace Chocopoi.DressingTools.Dynamics.Proxy
{
    internal class PhysBoneProxy : SingleRootDynamicsProxy
    {
        public static readonly System.Type PhysBoneType = DKEditorUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        public PhysBoneProxy(Component component)
        {
            Component = component;
            if (PhysBoneType == null)
            {
                throw new System.Exception("No VRCPhysBone component is found in this project. It is required to process PhysBone-based clothes.");
            }
        }

        public override Transform RootTransform
        {
            get
            {
                // if physbone root transform field is null, it implies it is controlling the current transform
                var rootTransform = (Transform)PhysBoneType.GetField("rootTransform").GetValue(Component);
                return rootTransform != null ? rootTransform : Component.transform;
            }
            set => PhysBoneType.GetField("rootTransform").SetValue(Component, value);
        }

        public override ICollection<Transform> IgnoreTransforms
        {
            get => (List<Transform>)PhysBoneType.GetField("ignoreTransforms").GetValue(Component);
            set => PhysBoneType.GetField("ignoreTransforms").SetValue(Component, value);
        }
    }
}
