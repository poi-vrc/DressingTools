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
    internal class VRMSpringBoneProxy : DynamicsProxy
    {
        public static readonly System.Type VRMSpringBoneType = DKEditorUtils.FindType("VRM.VRMSpringBone");

        public VRMSpringBoneProxy(Component component)
        {
            Component = component;
            if (VRMSpringBoneType == null)
            {
                throw new System.Exception("No VRMSpringBone component is found in this project. It is required to process VRMSpringBone-based dynamics.");
            }
        }

        public override ICollection<Transform> RootTransforms
        {
            get => (List<Transform>)VRMSpringBoneType.GetField("RootBones").GetValue(Component);
            set => VRMSpringBoneType.GetField("RootBones").SetValue(Component, value);
        }

        public override ICollection<Transform> IgnoreTransforms
        {
            get
            {
                Debug.LogWarning("[DressingFramework] VRM Spring Bone does not support IgnoreTransform, but this API was called!");
                return new List<Transform>();
            }
            set { Debug.LogWarning("[DressingFramework] VRM Spring Bone does not support IgnoreTransform, but this API was called!"); }
        }
    }
}
