using System.Collections.Generic;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public interface IDefaultDresserHook
    {
        bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings);
    }
}
