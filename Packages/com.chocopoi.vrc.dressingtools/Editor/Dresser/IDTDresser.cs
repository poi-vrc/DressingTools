using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.Dresser
{
    public interface IDTDresser
    {
        DTBoneMapping[] Execute(DTDresserSettings settings, out DTReport report);
    }
}
