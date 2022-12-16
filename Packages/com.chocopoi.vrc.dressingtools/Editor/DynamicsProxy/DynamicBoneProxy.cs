using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.DynamicsProxy
{
    public class DynamicBoneProxy
    {
        private static System.Type DynamicBoneType = DressingUtils.FindType("DynamicBone");

        public readonly Component component;

        public DynamicBoneProxy(Component component)
        {
            this.component = component;
            if (DynamicBoneType == null)
            {
                throw new System.Exception("No DynamicBone component is found in this project. It is required to process DynamicBone-based clothes.");
            }
        }

        public Transform transform
        {
            get { return component.transform; }
        }

        public GameObject gameObject
        {
            get { return component.gameObject; }
        }

        public Transform m_Root
        {
            get { return (Transform)DynamicBoneType.GetField("m_Root").GetValue(component); }
            set { DynamicBoneType.GetField("m_Root").SetValue(component, value); }
        }

        public List<Transform> m_Exclusions
        {
            get { return (List<Transform>)DynamicBoneType.GetField("m_Exclusions").GetValue(component); }
            set { DynamicBoneType.GetField("m_Exclusions").SetValue(component, value); }
        }
    }
}
