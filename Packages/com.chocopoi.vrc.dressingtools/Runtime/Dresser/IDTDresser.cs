using System.Collections.Generic;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;

namespace Chocopoi.DressingTools.Dresser
{
    public interface IDTDresser
    {
        DTDresserSettings DeserializeSettings(string serializedJson);
        DTDresserSettings NewSettings();
        DTReport Execute(DTDresserSettings settings, out List<DTBoneMapping> boneMappings);
    }
}
