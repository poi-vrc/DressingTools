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

using System;
using System.Collections.Generic;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Dynamics.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Dynamics
{
    internal static class DynamicsUtils
    {
        public static List<IDynamicsProxy> ScanDynamics(GameObject obj, Func<Component, bool> filterFunc = null)
        {
            var dynamicsList = new List<IDynamicsProxy>();

            // TODO: replace by reading YAML

            // get the dynbone type
            var DynamicBoneType = DKEditorUtils.FindType("DynamicBone");
            var PhysBoneType = DKEditorUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");
            var VRMSpringBoneType = DKEditorUtils.FindType("VRM.VRMSpringBone");

            // scan dynbones
            if (DynamicBoneType != null)
            {
                var dynBones = obj.GetComponentsInChildren(DynamicBoneType, true);
                foreach (var dynBone in dynBones)
                {
                    if (filterFunc != null && !filterFunc(dynBone))
                    {
                        continue;
                    }
                    dynamicsList.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan physbones
            if (PhysBoneType != null)
            {
                var physBones = obj.GetComponentsInChildren(PhysBoneType, true);
                foreach (var physBone in physBones)
                {
                    if (filterFunc != null && !filterFunc(physBone))
                    {
                        continue;
                    }
                    dynamicsList.Add(new PhysBoneProxy(physBone));
                }
            }

            // scan vrmspringbones (vrm0/univrm)
            if (VRMSpringBoneType != null)
            {
                var vrmSpringBones = obj.GetComponentsInChildren(VRMSpringBoneType, true);
                foreach (var vrmSpringBone in vrmSpringBones)
                {
                    if (filterFunc != null && !filterFunc(vrmSpringBone))
                    {
                        continue;
                    }
                    dynamicsList.Add(new VRMSpringBoneProxy(vrmSpringBone));
                }
            }

            return dynamicsList;
        }

        public static IDynamicsProxy FindDynamicsWithRoot(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            foreach (var bone in avatarDynamics)
            {
                if (bone.RootTransforms.Contains(dynamicsRoot))
                {
                    return bone;
                }
            }
            return null;
        }

        public static bool IsDynamicsExists(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            return FindDynamicsWithRoot(avatarDynamics, dynamicsRoot) != null;
        }
    }
}
