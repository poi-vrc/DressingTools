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

#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Serialization;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules.Integrations.VRChat
{
    /// <summary>
    /// VRChat merge animation layer wearable module config
    /// </summary>
    internal class VRCMergeAnimLayerWearableModuleConfig : IModuleConfig
    {
        /// <summary>
        /// Module identifier
        /// </summary>
        public const string ModuleIdentifier = "com.chocopoi.dressingtools.integration.vrchat.wearable.merge-anim-layer";

        /// <summary>
        /// Animation layer enum
        /// </summary>
        public enum AnimLayer
        {
            Base = 0,
            Additive = 1,
            Gesture = 2,
            Action = 3,
            FX = 4,
            Sitting = 5,
            TPose = 6,
            IKPose = 7
        }

        /// <summary>
        /// Path mode enum
        /// </summary>
        public enum PathMode
        {
            Relative = 0,
            Absolute = 1
        }

        /// <summary>
        ///  Current config version
        /// </summary>
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        /// <summary>
        /// Config version
        /// </summary>
        public SerializationVersion version;

        /// <summary>
        /// Animation layer to merge
        /// </summary>
        public AnimLayer animLayer;

        /// <summary>
        /// Path mode
        /// </summary>
        public PathMode pathMode;

        /// <summary>
        /// Remove animator after applying
        /// </summary>
        public bool removeAnimatorAfterApply;

        /// <summary>
        /// Match existing layers' write defaults setting
        /// </summary>
        public bool matchLayerWriteDefaults;

        /// <summary>
        /// Animator object path. If `null`, the module will use the wearable root to find the animator
        /// </summary>
        public string animatorPath;

        /// <summary>
        /// VRChat merge animator layer module config
        /// </summary>
        public VRCMergeAnimLayerWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            animLayer = AnimLayer.FX;
            pathMode = PathMode.Relative;
            removeAnimatorAfterApply = true;
            matchLayerWriteDefaults = true;
            animatorPath = "";
        }

        /// <summary>
        /// Convert internal animation layer type to VRCSDK type
        /// </summary>
        /// <param name="type">Internal type</param>
        /// <returns>VRCSDK type</returns>
        public static VRCAvatarDescriptor.AnimLayerType? ToAnimLayerType(AnimLayer type)
        {
            switch (type)
            {
                case AnimLayer.Base:
                    return VRCAvatarDescriptor.AnimLayerType.Base;
                case AnimLayer.Additive:
                    return VRCAvatarDescriptor.AnimLayerType.Additive;
                case AnimLayer.Gesture:
                    return VRCAvatarDescriptor.AnimLayerType.Gesture;
                case AnimLayer.Action:
                    return VRCAvatarDescriptor.AnimLayerType.Action;
                case AnimLayer.FX:
                    return VRCAvatarDescriptor.AnimLayerType.FX;
                case AnimLayer.Sitting:
                    return VRCAvatarDescriptor.AnimLayerType.Sitting;
                case AnimLayer.TPose:
                    return VRCAvatarDescriptor.AnimLayerType.TPose;
                case AnimLayer.IKPose:
                    return VRCAvatarDescriptor.AnimLayerType.IKPose;
            }
            return null;
        }
    }
}
#endif
