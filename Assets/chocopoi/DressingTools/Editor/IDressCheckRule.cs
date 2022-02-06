using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public interface IDressCheckRule
    {
        bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes);
    }
}
