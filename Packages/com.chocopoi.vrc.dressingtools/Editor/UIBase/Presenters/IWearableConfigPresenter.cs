using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;

namespace Chocopoi.DressingTools.UIBase.Presenters
{
    internal interface IWearableConfigPresenter
    {
        DTReport GenerateDresserMappings(IDTDresser dresser, DTDresserSettings dresserSettings);
        void StartMappingEditor();
    }
}
