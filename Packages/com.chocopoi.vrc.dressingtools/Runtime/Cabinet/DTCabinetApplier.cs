using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Cabinet
{
    public class DTCabinetApplier
    {
        public const string LogLabel = "DTCabinetApplier";

        public static class MessageCode
        {
            // Info
            public const string AdjustedWearablePositionFromDelta = "appliers.default.msgCode.info.adjustedWearablePositionFromDelta";
            public const string AdjustedWearableRotationFromDelta = "appliers.default.msgCode.info.adjustedWearableRotationFromDelta";
            public const string AdjustedAvatarScale = "appliers.default.msgCode.info.adjustedAvatarScale";
            public const string AdjustedWearableScale = "appliers.default.msgCode.info.adjustedWearableScale";

            // Error
            public const string UnableToDeserializeConfig = "appliers.default.msgCode.error.unableToDeserializeConfig";
            public const string ApplyingModuleHasErrors = "appliers.default.msgCode.error.applyingModuleHasErrors";
            public const string ApplyingWearableHasErrors = "appliers.default.msgCode.error.applyingWearableHasErrors";
        }

        private DTReport report;

        private DTCabinet cabinet;

        private List<IDynamicsProxy> avatarDynamics;

        public DTCabinetApplier(DTReport report, DTCabinet cabinet)
        {
            this.report = report;
            this.cabinet = cabinet;
        }

        private void ApplyTransforms(DTAvatarConfig avatarConfig, GameObject targetWearable, out Transform lastAvatarParent, out Vector3 lastAvatarScale)
        {
            var targetAvatar = cabinet.avatarGameObject;

            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    report.LogInfoLocalized(LogLabel, MessageCode.AdjustedWearablePositionFromDelta, wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    report.LogInfoLocalized(LogLabel, MessageCode.AdjustedWearableRotationFromDelta, wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            lastAvatarParent = cabinet.avatarGameObject.transform.parent;
            lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                cabinet.avatarGameObject.transform.SetParent(null);
            }

            var avatarScaleVec = avatarConfig.avatarLossyScale.ToVector3();
            if (targetAvatar.transform.localScale != avatarScaleVec)
            {
                report.LogInfoLocalized(LogLabel, MessageCode.AdjustedAvatarScale, avatarScaleVec.ToString());
                targetAvatar.transform.localScale = avatarScaleVec;
            }

            // apply wearable scale
            var wearableScaleVec = avatarConfig.wearableLossyScale.ToVector3();
            if (targetWearable.transform.localScale != wearableScaleVec)
            {
                report.LogInfoLocalized(LogLabel, MessageCode.AdjustedWearableScale, wearableScaleVec.ToString());
                targetWearable.transform.localScale = wearableScaleVec;
            }
        }

        private void RollbackTransform(Transform lastAvatarParent, Vector3 lastAvatarScale)
        {
            // restore avatar scale
            if (lastAvatarParent != null)
            {
                cabinet.avatarGameObject.transform.SetParent(lastAvatarParent);
            }
            cabinet.avatarGameObject.transform.localScale = lastAvatarScale;
        }

        private bool ApplyWearable(DTWearableConfig config, GameObject wearableGameObject)
        {
            // TODO: check config version and do migration here

            // TODO: do avatar GUID check?

            // TODO: is this check still necessary now?
            GameObject wearableObj;
            if (DTRuntimeUtils.IsGrandParent(cabinet.avatarGameObject.transform, wearableGameObject.transform))
            {
                //// check if it's a prefab
                //if (PrefabUtility.IsPartOfAnyPrefab(wearable.wearableGameObject))
                //{
                //    // unpack completely the prefab
                //    PrefabUtility.UnpackPrefabInstance(wearable.wearableGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                //}
                wearableObj = wearableGameObject;
            }
            else
            {
                // instantiate wearable prefab and parent to avatar
                wearableObj = Object.Instantiate(wearableGameObject, cabinet.avatarGameObject.transform);
            }

            // scan for wearable dynamics
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearableObj);

            // apply translation and scaling
            ApplyTransforms(config.targetAvatarConfig, wearableObj, out var lastAvatarParent, out var lastAvatarScale);

            // sort modules according to their apply order
            var modules = new List<DTWearableModuleBase>(config.modules);
            modules.Sort((m1, m2) => m1.ApplyOrder.CompareTo(m2.ApplyOrder));

            // do module apply
            foreach (var module in modules)
            {
                if (!module.Apply(report, cabinet, avatarDynamics, config, wearableGameObject))
                {
                    report.LogErrorLocalized(LogLabel, MessageCode.ApplyingModuleHasErrors);
                    return false;
                }
            }

            RollbackTransform(lastAvatarParent, lastAvatarScale);

            return true;
        }

        public void Execute()
        {
            // scan for avatar dynamics
            avatarDynamics = DTRuntimeUtils.ScanDynamics(cabinet.avatarGameObject);
            var wearables = cabinet.GetWearables();

            foreach (var wearable in wearables)
            {
                // deserialize the config
                var config = JsonConvert.DeserializeObject<DTWearableConfig>(wearable.configJson);

                if (config == null)
                {
                    report.LogErrorLocalized(LogLabel, MessageCode.UnableToDeserializeConfig);
                    continue;
                }

                if (!ApplyWearable(config, wearable.wearableGameObject))
                {
                    report.LogErrorLocalized(LogLabel, MessageCode.ApplyingWearableHasErrors, config.info.name);
                    break;
                }
            }
        }
    }
}
