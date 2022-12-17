using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class GroupRootObjectsHook : IDressHook
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            var toParent = new List<GameObject>();

            for (var i = 0; i < targetClothes.transform.childCount; i++)
            {
                var obj = targetClothes.transform.GetChild(i)?.gameObject;
                if (obj.name != settings.avatarArmatureObjectName)
                {
                    report.clothesAllObjects.Add(obj);
                    report.clothesMeshDataObjects.Add(obj);
                    toParent.Add(obj);
                }
            }

            if (settings.groupRootObjects)
            {
                var name = "DT_" + settings.clothesToDress.name;
                var clothesContainer = targetAvatar.transform.Find(name)?.gameObject;

                if (clothesContainer != null)
                {
                    report.errors |= DressCheckCodeMask.Error.EXISTING_CLOTHES_DETECTED;
                    return false;
                }

                clothesContainer = new GameObject(name);
                clothesContainer.transform.SetParent(targetAvatar.transform);

                foreach (var obj in toParent)
                {
                    obj.transform.SetParent(clothesContainer.transform);
                }
            }
            else
            {
                foreach (var obj in toParent)
                {
                    obj.name = settings.prefixToBeAdded + obj.name + settings.suffixToBeAdded;

                    if (targetAvatar.transform.Find(obj.name) != null)
                    {
                        report.errors |= DressCheckCodeMask.Error.EXISTING_CLOTHES_DETECTED;
                        return false;
                    }

                    obj.transform.SetParent(targetAvatar.transform);
                }
            }

            return true;
        }
    }
}
