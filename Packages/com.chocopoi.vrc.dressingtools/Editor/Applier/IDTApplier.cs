using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.Applier
{
    public interface IDTApplier
    {
        DTReport ApplyWearable(ApplierSettings settings, DTWearableConfig config);
    }
}
