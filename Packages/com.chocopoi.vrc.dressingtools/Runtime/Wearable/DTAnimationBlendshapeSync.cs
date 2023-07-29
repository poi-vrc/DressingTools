using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
{
    [Serializable]
    public class DTAnimationBlendshapeSync
    {
        public string avatarPath;
        public string avatarBlendshapeName;
        public float avatarFromValue;
        public float avatarToValue;
        public string wearablePath;
        public string wearableBlendshapeName;
        public float wearableFromValue;
        public float wearableToValue;
    }
}
