using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier
{
    public interface IDTApplier
    {
        DTApplierSettings DeserializeSettings(string serializedJson);
        DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet);
    }
}
