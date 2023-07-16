using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public class DTAnimationPreset
    {
        public DTAnimationToggle[] toggles;
        public DTAnimationBlendshapeValue[] blendshapes;

        public DTAnimationPreset()
        {
            toggles = new DTAnimationToggle[0];
            blendshapes = new DTAnimationBlendshapeValue[0];
        }
    }
}
