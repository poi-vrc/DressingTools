using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Proxy
{
    public class PhysBoneProxy
    {
        private static System.Type PhysBoneType = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        public readonly Component component;

        public PhysBoneProxy(Component component)
        {
            this.component = component;
            if (PhysBoneType == null)
            {
                throw new System.Exception("No VRCPhysBone component is found in this project. It is required to process PhysBone-based clothes.");
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

        public Transform rootTransform
        {
            get { return (Transform)PhysBoneType.GetField("rootTransform").GetValue(component); }
            set { PhysBoneType.GetField("rootTransform").SetValue(component, value); }
        }

        public List<Transform> ignoreTransforms
        {
            get { return (List<Transform>)PhysBoneType.GetField("ignoreTransforms").GetValue(component); }
            set { PhysBoneType.GetField("ignoreTransforms").SetValue(component, value); }
        }
    }
}
