using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
{
    [Serializable]
    public class DTAnimationPreset
    {
        public List<DTAnimationToggle> toggles;
        public List<DTAnimationBlendshapeValue> blendshapes;

        public DTAnimationPreset()
        {
            toggles = new List<DTAnimationToggle>();
            blendshapes = new List<DTAnimationBlendshapeValue>();
        }
    }
}
