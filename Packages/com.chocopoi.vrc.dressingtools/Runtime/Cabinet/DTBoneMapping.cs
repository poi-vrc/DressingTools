using System;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public enum DTBoneMappingType
    {
        DoNothing = 0,
        MoveToBone = 1,
        ParentConstraint = 2
    }

    [Serializable]
    public enum DTDynamicsBindingType
    {
        DoNothing = 0,
        ParentConstraint = 1,
        IgnoreTransform = 2,
        CopyDynamics = 3
    }

    [Serializable]
    public class DTGameObjectReference
    {
        public string path;
        public GameObject reference;
    }

    [Serializable]
    public class DTBoneMapping
    {
        public DTBoneMappingType mappingType;
        public DTDynamicsBindingType dynamicsBindingType;
        public string avatarBonePath;
        public string wearableBonePath;
    }
}
