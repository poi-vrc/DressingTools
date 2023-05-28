using Chocopoi.DressingTools.Cabinet;

namespace Chocopoi.DressingTools.Dresser
{
    internal interface IDTDresser
    {
        DTBoneMapping[] Execute(DTDresserSettings settings, out DTDresserReport report);
    }
}
