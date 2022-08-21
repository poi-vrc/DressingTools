using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chocopoi.DressingTools.Reporting;

namespace Chocopoi.DressingTools.Rules
{
    public interface IDressCheckRule
    {
        bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes);
    }
}
