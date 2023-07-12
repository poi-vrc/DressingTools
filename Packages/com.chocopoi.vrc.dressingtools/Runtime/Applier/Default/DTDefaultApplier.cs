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

                // check position delta and adjust
                {
                    var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                    if (wearableObj.transform.position - cabinet.avatarGameObject.transform.position != wearableWorldPos)
                    {
                        report.LogInfo(0, "Position delta mismatch, adjusting wearable position: " + wearableWorldPos.ToString());
                        wearableObj.transform.position += wearableWorldPos;
                    }
                }

                // check rotation delta and adjust
                {
                    var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                    if (wearableObj.transform.rotation * Quaternion.Inverse(cabinet.avatarGameObject.transform.rotation) != wearableWorldRot)
                    {
                        report.LogInfo(0, "Rotation delta mismatch, adjusting wearable rotation: " + wearableWorldRot.ToString());
                        wearableObj.transform.rotation *= wearableWorldRot;
                    }
                }

                // apply avatar scale
                var lastAvatarParent = cabinet.avatarGameObject.transform.parent;
                var lastAvatarScale = Vector3.zero + cabinet.avatarGameObject.transform.localScale;
                if (lastAvatarParent != null)
                {
                    // tricky workaround to apply lossy world scale is to unparent
                    cabinet.avatarGameObject.transform.SetParent(null);
                }
                cabinet.avatarGameObject.transform.localScale = avatarConfig.avatarLossyScale.ToVector3();

                // apply wearable scale
                wearableObj.transform.localScale = avatarConfig.wearableLossyScale.ToVector3();

                var boneMappings = new List<DTBoneMapping>();
                switch (wearableConfig.boneMappingMode)
                {
                    case DTWearableMappingMode.Auto:
                        var dresser = DresserRegistry.GetDresserByTypeName(wearableConfig.dresserName);
                        var dresserSettings = dresser.DeserializeSettings(wearableConfig.serializedDresserConfig);
                        if (dresserSettings == null)
                        {
                            // fallback to become a empty
                            dresserSettings = dresser.NewSettings();
                        }
                        break;
                }


                // restore avatar scale
                if (lastAvatarParent != null)
                {
                    cabinet.avatarGameObject.transform.SetParent(lastAvatarParent);
                }
                cabinet.avatarGameObject.transform.localScale = lastAvatarScale;
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
