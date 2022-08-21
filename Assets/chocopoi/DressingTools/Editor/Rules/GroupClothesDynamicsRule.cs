using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class GroupClothesDynamicsRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            if (!settings.groupDynamics || (report.clothesDynBones.Count == 0 && report.clothesPhysBones.Count == 0))
            {
                return true;
            }
            
            GameObject dynamicsContainer = new GameObject("DT_Dynamics");

            // Find the clothes container (if applicable)

            GameObject clothesContainer = null;
            if (settings.groupRootObjects)
            {
                string name = "DT_" + settings.clothesToDress.name;
                clothesContainer = targetAvatar.transform.Find(name)?.gameObject;
            }
            else
            {
                dynamicsContainer.name = settings.prefixToBeAdded + dynamicsContainer.name + settings.suffixToBeAdded;
                clothesContainer = targetClothes;
            }

            dynamicsContainer.transform.SetParent(clothesContainer.transform);

            // move all the found dynamics

            foreach (DTDynamicBone dynBone in report.clothesDynBones)
            {
                // in case it does not have a root transform
                if (dynBone.m_Root == null)
                {
                    dynBone.m_Root = dynBone.gameObject.transform;
                }

                UnityEditorInternal.ComponentUtility.CopyComponent(dynBone.component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(dynamicsContainer);

                // destroy the original one

                Object.DestroyImmediate(dynBone.component);
            }

            foreach (VRCPhysBone physBone in report.clothesPhysBones)
            {
                // in case it does not have a root transform
                if (physBone.rootTransform == null)
                {
                    physBone.rootTransform = physBone.gameObject.transform;
                }

                UnityEditorInternal.ComponentUtility.CopyComponent(physBone);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(dynamicsContainer);

                // destroy the original one

                Object.DestroyImmediate(physBone);
            }

            return true;
        }
    }
}
