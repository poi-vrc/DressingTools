using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chocopoi.DressingTools.Reporting;

namespace Chocopoi.DressingTools.Rules
{
    public class GroupRootObjectsRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            List<GameObject> toParent = new List<GameObject>();

            for (int i = 0; i < targetClothes.transform.childCount; i++)
            {
                GameObject obj = targetClothes.transform.GetChild(i)?.gameObject;
                if (obj.name != settings.avatarArmatureObjectName)
                {
                    report.clothesAllObjects.Add(obj);
                    report.clothesMeshDataObjects.Add(obj);
                    toParent.Add(obj);
                }
            }

            if (settings.groupRootObjects)
            {
                string name = "DT_" + settings.clothesToDress.name;
                GameObject clothesContainer = targetAvatar.transform.Find(name)?.gameObject;

                if (clothesContainer != null)
                {
                    report.errors |= DressCheckCodeMask.Error.EXISTING_CLOTHES_DETECTED;
                    return false;
                }

                clothesContainer = new GameObject(name);
                clothesContainer.transform.SetParent(targetAvatar.transform);

                foreach (GameObject obj in toParent)
                {
                    obj.transform.SetParent(clothesContainer.transform);
                }
            } else
            {
                foreach (GameObject obj in toParent)
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
