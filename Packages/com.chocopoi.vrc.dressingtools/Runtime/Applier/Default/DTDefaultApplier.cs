using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier.Default
{
    public class DTDefaultApplier : IDTApplier
    {
        private void ApplyTransforms(DTReport report, DTAvatarConfig avatarConfig, GameObject targetAvatar, GameObject targetWearable, out Transform lastAvatarParent, out Vector3 lastAvatarScale)
        {
            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    report.LogInfo(0, "Position delta mismatch, adjusting wearable position: " + wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    report.LogInfo(0, "Rotation delta mismatch, adjusting wearable rotation: " + wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            lastAvatarParent = targetAvatar.transform.parent;
            lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetAvatar.transform.SetParent(null);
            }
            targetAvatar.transform.localScale = avatarConfig.avatarLossyScale.ToVector3();

            // apply wearable scale
            targetWearable.transform.localScale = avatarConfig.wearableLossyScale.ToVector3();
        }

        private void RollbackTransform(GameObject targetAvatar, Transform lastAvatarParent, Vector3 lastAvatarScale)
        {
            // restore avatar scale
            if (lastAvatarParent != null)
            {
                targetAvatar.transform.SetParent(lastAvatarParent);
            }
            targetAvatar.transform.localScale = lastAvatarScale;
        }

        private bool GenerateMappings(DTReport report, DTWearableConfig wearableConfig, out List<DTBoneMapping> boneMappings, out List<DTObjectMapping> objectMappings)
        {
            // execute dresser
            var dresser = DresserRegistry.GetDresserByTypeName(wearableConfig.dresserName);
            var dresserSettings = dresser.DeserializeSettings(wearableConfig.serializedDresserConfig);
            if (dresserSettings == null)
            {
                // fallback to become a empty
                dresserSettings = dresser.NewSettings();
            }
            var dresserReport = dresser.Execute(dresserSettings, out boneMappings, out objectMappings);

            // abort on error
            if (dresserReport.Result != DTReportResult.Compatible && dresserReport.Result != DTReportResult.Ok)
            {
                report.LogError(0, string.Format("Unable to wear \"{0}\" with dresser report errors!", wearableConfig.info.name));
                return false;
            }

            // handle bone overrides
            if (wearableConfig.boneMappingMode == DTWearableMappingMode.Manual)
            {
                boneMappings = new List<DTBoneMapping>(wearableConfig.boneMappings);
            }
            else if (wearableConfig.boneMappingMode == DTWearableMappingMode.Override)
            {
                foreach (var mappingOverride in wearableConfig.boneMappings)
                {
                    foreach (var originalMapping in boneMappings)
                    {
                        // override on match
                        if (originalMapping.avatarBonePath == mappingOverride.avatarBonePath && originalMapping.wearableBonePath == mappingOverride.wearableBonePath)
                        {
                            originalMapping.mappingType = mappingOverride.mappingType;
                        }
                    }
                }
            }

            // handle object overrides
            if (wearableConfig.objectMappingMode == DTWearableMappingMode.Manual)
            {
                objectMappings = new List<DTObjectMapping>(wearableConfig.objectMappings);
            }
            else if (wearableConfig.objectMappingMode == DTWearableMappingMode.Override)
            {
                foreach (var mappingOverride in wearableConfig.objectMappings)
                {
                    foreach (var originalMapping in objectMappings)
                    {
                        // override on match
                        if (originalMapping.wearableObjectPath == mappingOverride.wearableObjectPath)
                        {
                            originalMapping.avatarObjectPath = mappingOverride.avatarObjectPath;
                        }
                    }
                }
            }

            return true;
        }

        public DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet)
        {
            var report = new DTReport();

            foreach (var wearableConfig in cabinet.wearables)
            {
                // TODO: check config version and do migration here

                // obtain avatar config
                DTAvatarConfig avatarConfig = null;
                {
                    var guid = DTRuntimeUtils.GetGameObjectOriginalPrefabGuid(cabinet.avatarGameObject);
                    if (guid == null || guid == "")
                    {
                        report.LogWarn(0, "Cannot find GUID of avatar, maybe not a prefab? Using the first found avatar config instead.");
                        avatarConfig = wearableConfig.targetAvatarConfigs[0];
                    }
                    else
                    {
                        avatarConfig = DTRuntimeUtils.FindAvatarConfigByGuid(wearableConfig.targetAvatarConfigs, guid);
                        if (avatarConfig == null)
                        {
                            report.LogWarn(0, string.Format("Wearable does not contain avatar config for the avatar GUID (\"{0}\") Using the first found avatar config instead.", guid));
                            avatarConfig = wearableConfig.targetAvatarConfigs[0];
                        }
                    }
                }

                // instantiate wearable prefab
                var wearableObj = Object.Instantiate(wearableConfig.wearableGameObject);

                ApplyTransforms(report, avatarConfig, cabinet.avatarGameObject, wearableObj, out var lastAvatarParent, out var lastAvatarScale);

                if (!GenerateMappings(report, wearableConfig, out var boneMappings, out var objectMappings))
                {
                    // abort on error
                    RollbackTransform(cabinet.avatarGameObject, lastAvatarParent, lastAvatarScale);
                    continue;
                }

                RollbackTransform(cabinet.avatarGameObject, lastAvatarParent, lastAvatarScale);
            }

            return report;
        }

        public DTApplierSettings DeserializeSettings(string serializedJson)
        {
            return JsonConvert.DeserializeObject<DTDefaultApplierSettings>(serializedJson);
        }

        public DTApplierSettings NewSettings()
        {
            return new DTDefaultApplierSettings();
        }
    }
}
