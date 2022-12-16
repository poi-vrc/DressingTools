using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class GroupClothesDynamicsHook : IDressHook
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            if (!settings.groupDynamics || (report.clothesDynamics.Count == 0))
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

            foreach (IDynamicsProxy dynamics in report.clothesDynamics)
            {
                // in case it does not have a root transform
                if (dynamics.RootTransform == null)
                {
                    dynamics.RootTransform = dynamics.GameObject.transform;
                }

                UnityEditorInternal.ComponentUtility.CopyComponent(dynamics.Component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(dynamicsContainer);

                // destroy the original one

                Object.DestroyImmediate(dynamics.Component);
            }

            return true;
        }
    }
}
