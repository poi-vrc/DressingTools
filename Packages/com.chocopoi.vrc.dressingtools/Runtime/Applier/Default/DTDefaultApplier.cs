using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json;
using UnityEditor;

namespace Chocopoi.DressingTools.Applier.Default
{
    public class DTDefaultApplier : IDTApplier
    {
        public object DTUtils { get; private set; }

        private DTAvatarConfig FindAvatarConfigByGuid(DTAvatarConfig[] configs, string guid)
        {
            foreach (var avatarConfig in configs)
            {
                if (avatarConfig.guid == guid)
                {
                    return avatarConfig;
                }
            }
            return null;
        }

        public DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet)
        {
            var report = new DTReport();

            foreach (var wearable in cabinet.wearables)
            {
                // TODO: check config version and do migration here

                // obtain avatar config
                DTAvatarConfig avatarConfig = null;
                {
                    var guid = DTRuntimeUtils.GetGameObjectOriginalPrefabGuid(cabinet.avatarGameObject);
                    if (guid == null || guid == "")
                    {
                        report.LogWarn(0, "Cannot find GUID of avatar, maybe not a prefab? Using the first found avatar config instead.");
                        avatarConfig = wearable.targetAvatarConfigs[0];
                    }
                    else
                    {
                        avatarConfig = FindAvatarConfigByGuid(wearable.targetAvatarConfigs, guid);
                        if (avatarConfig == null)
                        {
                            report.LogWarn(0, string.Format("Wearable does not contain avatar config for the avatar GUID (\"{0}\") Using the first found avatar config instead.", guid));
                            avatarConfig = wearable.targetAvatarConfigs[0];
                        }
                    }
                }
            }

            return report;
        }

        public DTApplierSettings DeserializeSettings(string serializedJson)
        {
            return JsonConvert.DeserializeObject<DTDefaultApplierSettings>(serializedJson);
        }
    }
}
