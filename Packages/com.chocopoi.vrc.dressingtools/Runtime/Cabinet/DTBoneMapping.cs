using System;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public enum DTBoneMappingType
    {
        DoNothing = 0,
        MoveToBone = 1,
        ParentConstraint = 2,
        IgnoreTransform = 3,
        CopyDynamics = 4
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
        public string avatarBonePath;
        public string wearableBonePath;

        public bool Equals(DTBoneMapping x)
        {
            return mappingType == x.mappingType && avatarBonePath == x.avatarBonePath && wearableBonePath == x.wearableBonePath;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} -> {2}", mappingType, wearableBonePath, avatarBonePath);
        }
    }
}
