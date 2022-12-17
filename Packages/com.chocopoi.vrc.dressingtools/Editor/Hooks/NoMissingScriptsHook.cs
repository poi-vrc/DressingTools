using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class NoMissingScriptsHook : IDressHook
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

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            var result = true;

            //scan avatar missing scripts
            result &= ScanGameObject(targetAvatar);

            if (!result)
            {
                report.errors |= DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_AVATAR;
            }

            //scan clothes missing scripts
            result &= ScanGameObject(targetClothes);

            if (!result)
            {
                report.errors |= DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_CLOTHES;
            }

            return result;
        }
    }
}
