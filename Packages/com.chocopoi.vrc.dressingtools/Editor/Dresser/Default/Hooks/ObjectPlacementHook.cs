using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public class ObjectPlacementHook : IDefaultDresserHook
    {
        public bool SearchTarget(Transform toSearch, Transform target)
        {
            var result = false;
            for (var i = 0; i < toSearch.childCount; i++)
            {
                var child = toSearch.GetChild(i);

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

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings)
        {
            // search is wearable inside the avatar
            if (SearchTarget(settings.targetAvatar.transform, settings.targetWearable.transform))
            {
                report.LogError(DTDefaultDresser.MessageCode.WearableInsideAvatar, "Wearable is inside avatar, please move it away.");
                return false;
            }

            // search is avatar inside the wearable
            if (SearchTarget(settings.targetWearable.transform, settings.targetAvatar.transform))
            {
                report.LogError(DTDefaultDresser.MessageCode.AvatarInsideWearable, "Avatar is inside wearable, please move it away.");
                return false;
            }

            return true;
        }
    }
}
