using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public interface IDressHook
    {
        bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes);
    }
}
