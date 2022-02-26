﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressReport
    {
        private static readonly IDressCheckRule[] rules = new IDressCheckRule[]
        {
            new NotAPrefabRule(),
            new ExistingPrefixSuffixRule(),
            new ArmatureRule(),
            new MeshDataRule(),
            new TestModeRule()
        };

        public DressCheckResult result;

        public DressCheckCodeMask.Info infos;

        public DressCheckCodeMask.Warn warnings;

        public DressCheckCodeMask.Error errors;

        private DressReport()
        {

        }

        private static void CleanUp()
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("DressingToolsPreview_"))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        public static DressReport GenerateReport(DressSettings settings)
        {
            return Execute(settings, false);
        }

        public static DressReport Execute(DressSettings settings, bool write)
        {
            CleanUp();

            DressReport report = new DressReport();

            if (settings.activeAvatar == null || settings.clothesToDress == null)
            {
                report.result = DressCheckResult.INVALID_SETTINGS;
                report.errors |= DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES;
                return report;
            }

            string avatarNewName = "DressingToolsPreview_" + settings.activeAvatar.gameObject.name;
            string clothesNewName = "DressingToolsPreview_" + settings.clothesToDress.name;

            GameObject targetAvatar;
            GameObject targetClothes;

            if (!write)
            {
                targetAvatar = Object.Instantiate(settings.activeAvatar.gameObject);
                targetAvatar.name = avatarNewName;

                Vector3 newAvatarPosition = targetAvatar.transform.position;
                newAvatarPosition.x -= 20;
                targetAvatar.transform.position = newAvatarPosition;

                targetClothes = Object.Instantiate(settings.clothesToDress);
                targetClothes.name = clothesNewName;

                Vector3 newClothesPosition = targetClothes.transform.position;
                newClothesPosition.x -= 20;
                targetClothes.transform.position = newClothesPosition;
            } else
            {
                targetAvatar = settings.activeAvatar.gameObject;
                targetClothes = settings.clothesToDress;
            }

            foreach (IDressCheckRule rule in rules)
            {
                if (!rule.Evaluate(report, settings, targetAvatar, targetClothes))
                {
                    break;
                }
            }

            if (report.errors > 0)
            {
                report.result = DressCheckResult.INCOMPATIBLE;
            } else if (report.warnings > 0)
            {
                report.result = DressCheckResult.COMPATIBLE;
            } else
            {
                report.result = DressCheckResult.OK;
            }

#if UNITY_EDITOR
            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
#endif
            return report;
        }
    }
}
