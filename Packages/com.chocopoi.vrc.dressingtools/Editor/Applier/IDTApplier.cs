using Chocopoi.DressingTools.Cabinet;

namespace Chocopoi.DressingTools.Applier
{
    internal interface IDTApplier
    {
        ApplierReport ApplyWearable(ApplierSettings settings, DTWearableConfig config);
    }
}
