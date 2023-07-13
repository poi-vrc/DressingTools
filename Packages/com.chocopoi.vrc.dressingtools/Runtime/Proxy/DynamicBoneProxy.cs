using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Proxy
{
    public class DynamicBoneProxy : IDynamicsProxy
    {
        public static readonly System.Type DynamicBoneType = DTRuntimeUtils.FindType("DynamicBone");

        public DynamicBoneProxy(Component component)
        {
            Component = component;
            if (DynamicBoneType == null)
            {
                throw new System.Exception("No DynamicBone component is found in this project. It is required to process DynamicBone-based clothes.");
            }
        }

        public Component Component { get; }

        public Transform Transform
        {
            get { return Component.transform; }
        }

        public GameObject GameObject
        {
            get { return Component.gameObject; }
        }

        public Transform RootTransform
        {
            get { return (Transform)DynamicBoneType.GetField("m_Root").GetValue(Component); }
            set { DynamicBoneType.GetField("m_Root").SetValue(Component, value); }
        }

        public List<Transform> IgnoreTransforms
        {
            get { return (List<Transform>)DynamicBoneType.GetField("m_Exclusions").GetValue(Component); }
            set { DynamicBoneType.GetField("m_Exclusions").SetValue(Component, value); }
        }
    }
}
