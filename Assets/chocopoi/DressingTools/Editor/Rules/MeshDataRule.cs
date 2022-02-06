using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class MeshDataRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            List<GameObject> toParent = new List<GameObject>();

            for (int i = 0; i < targetClothes.transform.childCount; i++)
            {
                GameObject obj = targetClothes.transform.GetChild(i)?.gameObject;
                if (obj.name != "Armature")
                {
                    toParent.Add(obj);
                }
            }

            foreach (GameObject obj in toParent)
            { 
                obj.name = settings.prefixToBeAdded + obj.name + settings.suffixToBeAdded;
                obj.transform.SetParent(targetAvatar.transform);
            }

            return true;
        }
    }
}
