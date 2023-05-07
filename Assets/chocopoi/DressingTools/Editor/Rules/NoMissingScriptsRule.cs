using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Rules
{
    public class NoMissingScriptsRule : IDressCheckRule
    {
        private int RemoveAllMissingScripts(List<GameObject> list)
        {
            int count = 0;
            foreach (var obj in list)
            {
                count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }
            return count;
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            bool dynBoneInstalled = DressingUtils.FindType("DynamicBone") != null;

            //scan avatar missing scripts
            var avatarMissingScriptObjects = new List<GameObject>();
            MissingScriptsChecker.ScanGameObject(targetAvatar, avatarMissingScriptObjects);

            if (avatarMissingScriptObjects.Count > 0)
            {
                Debug.LogWarning("[DressingTools] Missing script detected in avatar, make sure you have imported DynamicBones or related stuff.");
                if (dynBoneInstalled)
                {
                    report.warnings |= DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_AVATAR_WILL_BE_REMOVED;
                    int count = RemoveAllMissingScripts(avatarMissingScriptObjects);
                    Debug.Log("[DressingTools] Removed " + count + "/" + avatarMissingScriptObjects.Count + " avatar missing script(s).");
                }
                else
                {
                    report.errors |= DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_AVATAR;
                }
            }

            //scan clothes missing scripts
            var clothesMissingScriptObjects = new List<GameObject>();
            MissingScriptsChecker.ScanGameObject(targetClothes, clothesMissingScriptObjects);

            if (clothesMissingScriptObjects.Count > 0)
            {
                Debug.LogWarning("[DressingTools] Missing script detected in clothes, make sure you have imported DynamicBones or related stuff.");
                if (dynBoneInstalled)
                {
                    report.warnings |= DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_CLOTHES_WILL_BE_REMOVED;
                    int count = RemoveAllMissingScripts(clothesMissingScriptObjects);
                    Debug.Log("[DressingTools] Removed " + count + "/" + clothesMissingScriptObjects.Count + " clothes missing script(s).");
                }
                else
                {
                    report.errors |= DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_CLOTHES;
                }
            }

            return dynBoneInstalled || (avatarMissingScriptObjects.Count == 0 && clothesMissingScriptObjects.Count == 0);
        }
    }
}
