using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressSettings
    {
        public VRC.SDKBase.VRC_AvatarDescriptor activeAvatar;

        public GameObject clothesToDress;

        public string prefixToBeAdded;

        public string suffixToBeAdded;

        public bool removeExistingPrefixSuffix;

        public int dynamicBoneOption;

        public bool groupBones;

        public bool groupRootObjects;

        public bool groupDynamics;

        public string armatureObjectName = "Armature";
    }
}
