using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class NotAPrefabHook : IDressHook
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(settings.clothesToDress) &&
                PrefabUtility.GetPrefabInstanceStatus(settings.clothesToDress) != PrefabInstanceStatus.NotAPrefab)
            {
                report.errors |= DressCheckCodeMask.Error.CLOTHES_IS_A_PREFAB;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
