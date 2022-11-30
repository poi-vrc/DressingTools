using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Rules
{
    public interface IDressCheckRule
    {
        bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes);
    }
}
