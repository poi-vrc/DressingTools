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

        public bool Equals(DTBoneMapping x)
        {
            return mappingType == x.mappingType && dynamicsBindingType == x.dynamicsBindingType && avatarBonePath == x.avatarBonePath && wearableBonePath == x.wearableBonePath;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2} -> {3}", mappingType, dynamicsBindingType, wearableBonePath, avatarBonePath);
        }
    }
}
