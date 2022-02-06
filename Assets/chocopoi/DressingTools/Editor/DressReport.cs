using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressReport
    {
        private static readonly IDressCheckRule[] rules = new IDressCheckRule[]
        {
            new NotAPrefabRule(),
            new ArmatureRule(),
            new MeshDataRule()
        };

        public DressCheckResult result;

        public DressCheckCodeMask.Info infos;

        public DressCheckCodeMask.Warn warnings;

        public DressCheckCodeMask.Error errors;

        private DressReport()
        {

        }

        public static DressReport GenerateReport(DressSettings settings)
        {
            return Execute(settings, false);
        }

        public static DressReport Execute(DressSettings settings, bool write)
        {
            DressReport report = new DressReport();

            if (settings.activeAvatar == null || settings.clothesToDress == null)
            {
                report.result = DressCheckResult.INVALID_SETTINGS;
                report.errors |= DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES;
                return report;
            }

            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("DressingToolsPreview_"))
                {
                    Object.DestroyImmediate(obj);
                }
            }

            string avatarNewName = "DressingToolsPreview_" + settings.activeAvatar.gameObject.name;
            string clothesNewName = "DressingToolsPreview_" + settings.clothesToDress.name;

            GameObject targetAvatar;
            GameObject targetClothes;

            if (!write)
            {
                targetAvatar = Object.Instantiate(settings.activeAvatar.gameObject);
                targetAvatar.name = avatarNewName;

                targetClothes = Object.Instantiate(settings.clothesToDress);
                targetClothes.name = clothesNewName;
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
                report.result = DressCheckResult.IMCOMPATIBLE;
            } else if (report.warnings > 0)
            {
                report.result = DressCheckResult.COMPATIBLE;
            } else
            {
                report.result = DressCheckResult.OK;
            }

            return report;
        }
    }
}
