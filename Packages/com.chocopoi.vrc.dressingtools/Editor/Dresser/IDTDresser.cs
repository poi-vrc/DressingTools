using Chocopoi.DressingTools.Cabinet;

namespace Chocopoi.DressingTools.Dresser
{
    public interface IDTDresser
    {
        DTBoneMapping[] Execute(DTDresserSettings settings, out DTDresserReport report);
    }
}
