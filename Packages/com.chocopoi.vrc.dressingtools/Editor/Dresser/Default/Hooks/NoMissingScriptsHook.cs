using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    // TODO: replace by reading missing scripts Unity files
    public class NoMissingScriptsHook : IDefaultDresserHook
    {
        public bool ScanGameObject(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogError("[DressingTools] Missing script detected, make sure you have imported DynamicBones or related stuff.");
                    return false;
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                if (!ScanGameObject(child.gameObject))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings)
        {
            var result = true;

            //scan avatar missing scripts
            result &= ScanGameObject(settings.targetAvatar);

            if (!result)
            {
                report.LogError(DTDefaultDresser.MessageCode.MissingScriptsDetectedInAvatar, "Missing scripts detected in avatar");
            }

            //scan wearable missing scripts
            result &= ScanGameObject(settings.targetWearable);

            if (!result)
            {
                report.LogError(DTDefaultDresser.MessageCode.MissingScriptsDetectedInWearable, "Missing scripts detected in wearable");
            }

            return result;
        }
    }
}
