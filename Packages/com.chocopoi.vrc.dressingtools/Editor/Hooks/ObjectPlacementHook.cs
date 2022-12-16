using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class ObjectPlacementHook : IDressHook
    {
        public bool SearchTarget(Transform toSearch, Transform target)
        {
            bool result = false;
            for (int i = 0; i < toSearch.childCount; i++)
            {
                Transform child = toSearch.GetChild(i);

                if (child == target)
                {
                    return true;
                }

                if (result = SearchTarget(child, target))
                {
                    return true;
                }
            }
            return result;
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            // search is clothes inside the avatar
            if (SearchTarget(settings.activeAvatar.gameObject.transform, settings.clothesToDress.transform))
            {
                report.errors |= DressCheckCodeMask.Error.CLOTHES_INSIDE_AVATAR;
                return false;
            }

            // search is avatar inside the clothes
            if (SearchTarget(settings.clothesToDress.transform, settings.activeAvatar.gameObject.transform))
            {
                report.errors |= DressCheckCodeMask.Error.AVATAR_INSIDE_CLOTHES;
                return false;
            }

            return true;
        }
    }
}
