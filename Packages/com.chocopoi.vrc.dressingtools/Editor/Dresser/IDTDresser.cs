using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.Dresser
{
    public interface IDTDresser
    {
        DTReport Execute(DTDresserSettings settings, out List<DTBoneMapping> boneMappings, out List<DTObjectMapping> objectMappings);
    }
}
