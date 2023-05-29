using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public interface IDefaultDresserHook
    {
        bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings);
    }
}
