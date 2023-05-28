using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public enum DTWearableCustomizableType
    {
        Toggle = 0,
        Blendshape = 1
    }

    [Serializable]
    public class DTWearableCustomizable
    {
        public DTWearableCustomizableType type;
        public DTAnimationToggle[] avatarRequiredToggles;
        public DTAnimationToggle[] wearableToggles;
        public DTAnimationBlendshapeValue[] avatarRequiredBlendshapes;
        public DTAnimationBlendshapeValue[] wearableBlendshapes;
    }
}
