using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    // TODO: replace by reading missing scripts Unity files
    public class NoMissingScriptsHook : IDefaultDresserHook
    {
        public bool ScanGameObject(DTReport report, int errorCode, GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.LogError(errorCode, "Missing script detected, make sure you have imported DynamicBones or related stuff: " + gameObject.name);
                    return false;
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                if (!ScanGameObject(report, errorCode, child.gameObject))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings, List<DTObjectMapping> objectMappings)
        {
            //scan avatar missing scripts
            var avatarResult = ScanGameObject(report, DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar, settings.targetAvatar);

            if (!avatarResult)
            {
                report.LogError(DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar, "Missing scripts detected in avatar");
            }

            //scan wearable missing scripts
            var clothesResult = ScanGameObject(report, DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable, settings.targetWearable);

            if (!clothesResult)
            {
                report.LogError(DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable, "Missing scripts detected in wearable");
            }

            return avatarResult && clothesResult;
        }
    }
}
