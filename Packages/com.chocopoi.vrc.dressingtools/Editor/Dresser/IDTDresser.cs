using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.Dresser
{
    public interface IDTDresser
    {
        List<DTBoneMapping> Execute(DTDresserSettings settings, out DTReport report);
    }
}
