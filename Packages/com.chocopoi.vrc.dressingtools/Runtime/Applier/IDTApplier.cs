using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier
{
    public interface IDTApplier
    {
        DTApplierSettings DeserializeSettings(string serializedJson);
        DTApplierSettings NewSettings();
        DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet);
    }
}
