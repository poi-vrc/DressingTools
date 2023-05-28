using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public interface IDefaultDresserHook
    {
        bool Evaluate(DTDresserReport report, DTDresserSettings settings);
    }
}
