using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.DressCheckCodeMask
{
    public enum Info
    {
        //Info
        NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED = 0x01,
        DYNAMIC_BONE_ALL_IGNORED = 0x02,
    }

    public enum Warn
    {
        //Warning
        MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL = 0x01,
        MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL = 0x02,
        BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL = 0x04,
    }

    public enum Error
    {
        //Errors
        NO_ARMATURE_IN_AVATAR = 0x01,
        NO_ARMATURE_IN_CLOTHES = 0x02,
        NULL_ACTIVE_AVATAR_OR_CLOTHES = 0x04,
        NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL = 0x08,
        NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL = 0x10,
        CLOTHES_IS_A_PREFAB = 0x20,
    }
}
