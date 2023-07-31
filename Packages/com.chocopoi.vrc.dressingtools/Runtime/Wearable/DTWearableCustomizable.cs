using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
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
        public List<DTAnimationToggle> avatarRequiredToggles;
        public List<DTAnimationToggle> wearableToggles;
        public List<DTAnimationBlendshapeValue> avatarRequiredBlendshapes;
        public List<DTAnimationBlendshapeValue> wearableBlendshapes;

        public DTWearableCustomizable()
        {
            type = DTWearableCustomizableType.Toggle;
            avatarRequiredToggles = new List<DTAnimationToggle>();
            wearableToggles = new List<DTAnimationToggle>();
            avatarRequiredBlendshapes = new List<DTAnimationBlendshapeValue>();
            wearableBlendshapes = new List<DTAnimationBlendshapeValue>();
        }
    }
}
