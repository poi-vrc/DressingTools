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
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Wearable
{
    internal static class WearableConfigEditorExtensions
    {
        public static List<WearableModule> FindUnknownModules(this WearableConfig config)
        {
            var list = new List<WearableModule>();
            foreach (var module in config.modules)
            {
                var provider = ModuleManager.Instance.GetWearableModuleProvider(module.moduleName);
                if (provider == null)
                {
                    list.Add(module);
                }
            }
            return list;
        }

        public static void ApplyAvatarConfigTransforms(this WearableConfig config, GameObject targetAvatar, GameObject targetWearable)
        {
            // check position delta and adjust
            {
                var wearableWorldPos = config.avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    Debug.LogFormat("[DressingTools] [AddCabinetWearable] Moved wearable world pos: {0}", wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = config.avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    Debug.LogFormat("[DressingTools] [AddCabinetWearable] Moved wearable world rotation: {0}", wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            var lastAvatarParent = targetAvatar.transform.parent;
            var lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetAvatar.transform.SetParent(null);
            }

            var avatarScaleVec = config.avatarConfig.avatarLossyScale.ToVector3();
            if (targetAvatar.transform.localScale != avatarScaleVec)
            {
                Debug.LogFormat("[DressingTools] [AddCabinetWearable] Adjusted avatar scale: {0}", avatarScaleVec.ToString());
                targetAvatar.transform.localScale = avatarScaleVec;
            }

            // apply wearable scale
            var lastWearableParent = targetWearable.transform.parent;
            var lastWearableScale = Vector3.zero + targetWearable.transform.localScale;
            if (lastWearableParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetWearable.transform.SetParent(null);
            }

            var wearableScaleVec = config.avatarConfig.wearableLossyScale.ToVector3();
            if (targetWearable.transform.localScale != wearableScaleVec)
            {
                Debug.LogFormat("[DressingTools] [AddCabinetWearable] Adjusted wearable scale: {0}", wearableScaleVec.ToString());
                targetWearable.transform.localScale = wearableScaleVec;
            }

            // restore avatar scale
            if (lastAvatarParent != null)
            {
                targetAvatar.transform.SetParent(lastAvatarParent);
            }
            targetAvatar.transform.localScale = lastAvatarScale;

            // restore wearable scale
            if (lastWearableParent != null)
            {
                targetWearable.transform.SetParent(lastWearableParent);
            }
            targetWearable.transform.localScale = lastWearableScale;
        }
    }
}
