using System.Collections.Generic;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    // TODO: replace by reading missing scripts Unity files
    public class NoMissingScriptsHook : IDefaultDresserHook
    {
        public bool ScanGameObject(DTReport report, string errorCode, GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.LogErrorLocalized(DTDefaultDresser.LogLabel, errorCode, gameObject.name);
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

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings)
        {
            //scan avatar missing scripts
            var avatarResult = ScanGameObject(report, DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar, settings.targetAvatar);

            if (!avatarResult)
            {
                report.LogErrorLocalized(DTDefaultDresser.LogLabel, DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar);
            }

            //scan wearable missing scripts
            var clothesResult = ScanGameObject(report, DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable, settings.targetWearable);

            if (!clothesResult)
            {
                report.LogErrorLocalized(DTDefaultDresser.LogLabel, DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable);
            }

            return avatarResult && clothesResult;
        }
    }
}
